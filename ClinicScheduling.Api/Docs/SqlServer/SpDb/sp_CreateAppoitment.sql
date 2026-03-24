CREATE OR ALTER PROCEDURE dbo.sp_CreateAppointment
    @Id UNIQUEIDENTIFIER,
    @DoctorId UNIQUEIDENTIFIER,
    @PatientId UNIQUEIDENTIFIER,
    @StartDateTime DATETIME2,
    @EndDateTime DATETIME2,
    @DurationMinutes INT = 0,
    @Status NVARCHAR(50) = 'Scheduled',
    @CreatedAt DATETIME2 = NULL,
    @ModifiedAt DATETIME2 = NULL,
    @IsActive BIT = 1
    AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;

BEGIN TRY
BEGIN TRANSACTION;

        DECLARE @AppointmentDurationMinutes INT;
        DECLARE @ExpectedEndDateTime DATETIME2;
        DECLARE @AppointmentDayOfWeek INT;
        DECLARE @StartTime TIME;
        DECLARE @EndTime TIME;

        IF @CreatedAt IS NULL
            SET @CreatedAt = SYSUTCDATETIME();

        IF @ModifiedAt IS NULL
            SET @ModifiedAt = SYSUTCDATETIME();

SELECT TOP 1
            @AppointmentDurationMinutes = S.AppointmentDurationMinutes
FROM Doctors D
         INNER JOIN Specialties S ON S.Id = D.SpecialtyId
WHERE D.Id = @DoctorId
  AND D.IsActive = 1;

IF @AppointmentDurationMinutes IS NULL
            THROW 50001, 'No se encontró un doctor activo para la cita.', 1;

        IF @EndDateTime <= @StartDateTime
            THROW 50002, 'EndDateTime debe ser mayor que StartDateTime.', 1;

        SET @ExpectedEndDateTime = DATEADD(MINUTE, @AppointmentDurationMinutes, @StartDateTime);

        IF @DurationMinutes <> 0 AND @DurationMinutes <> @AppointmentDurationMinutes
            THROW 50003, 'La duración de la cita para la especialidad del doctor no es válida.', 1;

        IF @EndDateTime <> @ExpectedEndDateTime
            THROW 50004, 'La hora de fin no coincide con la duración esperada de la cita.', 1;

        SET DATEFIRST 1;
        SET @AppointmentDayOfWeek = DATEPART(WEEKDAY, @StartDateTime);
        SET @StartTime = CAST(@StartDateTime AS TIME);
        SET @EndTime = CAST(@ExpectedEndDateTime AS TIME);

        IF NOT EXISTS
        (
            SELECT 1
            FROM DoctorSchedules DS
            WHERE DS.DoctorId = @DoctorId
              AND DS.IsActive = 1
              AND DS.DayOfWeek = @AppointmentDayOfWeek
              AND @StartTime >= DS.StartTime
              AND @EndTime <= DS.EndTime
        )
            THROW 50005, 'No se puede agendar una cita con el doctor porque no está dentro del horario laboral registrado.', 1;

        IF EXISTS
        (
            SELECT 1
            FROM Appointments A WITH (UPDLOCK, HOLDLOCK)
            WHERE A.IsActive = 1
              AND A.DoctorId = @DoctorId
              AND A.Status = 'Scheduled'
              AND A.StartDateTime < @ExpectedEndDateTime
              AND A.EndDateTime > @StartDateTime
        )
            THROW 50006, 'No se puede agendar la cita porque el doctor ya tiene una cita reservada en ese horario.', 1;

        SET @DurationMinutes = @AppointmentDurationMinutes;
        SET @EndDateTime = @ExpectedEndDateTime;

INSERT INTO Appointments
(
    Id,
    DoctorId,
    PatientId,
    StartDateTime,
    EndDateTime,
    DurationMinutes,
    Status,
    CreatedAt,
    ModifiedAt,
    IsActive
)
VALUES
    (
        @Id,
        @DoctorId,
        @PatientId,
        @StartDateTime,
        @EndDateTime,
        @DurationMinutes,
        @Status,
        @CreatedAt,
        @ModifiedAt,
        @IsActive
    );

COMMIT TRANSACTION;

SELECT TOP 1 *
FROM Appointments
WHERE Id = @Id;
END TRY
BEGIN CATCH
IF @@TRANCOUNT > 0
            ROLLBACK TRANSACTION;
        THROW;
END CATCH
END
