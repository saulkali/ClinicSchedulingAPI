CREATE OR ALTER PROCEDURE dbo.sp_GetDoctorAvailability
    @DoctorId UNIQUEIDENTIFIER,
    @DayOfWeek INT
AS
BEGIN
    SET NOCOUNT ON;

    IF @DayOfWeek NOT BETWEEN 1 AND 7
        THROW 50011, 'DayOfWeek debe estar entre 1 (lunes) y 7 (domingo).', 1;

    DECLARE @AppointmentDurationMinutes INT;

    SELECT TOP 1
        @AppointmentDurationMinutes = S.AppointmentDurationMinutes
    FROM Doctors D
        INNER JOIN Specialties S ON S.Id = D.SpecialtyId
    WHERE D.Id = @DoctorId
      AND D.IsActive = 1
      AND S.IsActive = 1;

    IF @AppointmentDurationMinutes IS NULL
        THROW 50012, 'No se encontró un doctor activo con especialidad válida.', 1;

    ;WITH ActiveSchedules AS
    (
        SELECT
            DS.DoctorId,
            DS.DayOfWeek,
            DS.StartTime,
            DS.EndTime,
            DATEDIFF(MINUTE, CAST('00:00:00' AS TIME), DS.StartTime) AS StartMinute,
            DATEDIFF(MINUTE, CAST('00:00:00' AS TIME), DS.EndTime) AS EndMinute
        FROM DoctorSchedules DS
        WHERE DS.DoctorId = @DoctorId
          AND DS.DayOfWeek = @DayOfWeek
          AND DS.IsActive = 1
    ),
    NumberSeries AS
    (
        SELECT 0 AS N
        UNION ALL
        SELECT N + 1
        FROM NumberSeries
        WHERE N < 300
    )
    SELECT
        S.DoctorId,
        S.DayOfWeek,
        CAST(DATEADD(MINUTE, N.N * @AppointmentDurationMinutes, CAST(S.StartTime AS DATETIME2)) AS TIME) AS StartTime,
        CAST(DATEADD(MINUTE, (N.N + 1) * @AppointmentDurationMinutes, CAST(S.StartTime AS DATETIME2)) AS TIME) AS EndTime,
        @AppointmentDurationMinutes AS DurationMinutes
    FROM ActiveSchedules S
        INNER JOIN NumberSeries N ON S.StartMinute + ((N.N + 1) * @AppointmentDurationMinutes) <= S.EndMinute
    ORDER BY S.StartTime, StartTime
    OPTION (MAXRECURSION 1000);
END
