CREATE OR ALTER PROCEDURE dbo.sp_GetDoctorAvailability
    @DoctorId UNIQUEIDENTIFIER,
    @Date DATE
    AS
BEGIN    

    IF @Date IS NULL
        THROW 50011, 'Date es requerido.', 1;

    SET DATEFIRST 1;
    DECLARE @DayOfWeek INT = DATEPART(WEEKDAY, @Date);

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
                         DS.StartTime,
                         DS.EndTime,
                         DATEDIFF(MINUTE, CAST('00:00:00' AS TIME), DS.StartTime) AS StartMinute,
                         DATEDIFF(MINUTE, CAST('00:00:00' AS TIME), DS.EndTime) AS EndMinute
                     FROM DoctorSchedules DS
                     WHERE DS.DoctorId = @DoctorId
                       AND DS.DayOfWeek = @DayOfWeek
                       AND DS.IsActive = 1
                 ),
          BusyAppointments AS
                 (
                     SELECT
                         A.DoctorId,
                         A.StartDateTime,
                         A.EndDateTime
                     FROM Appointments A
                     WHERE A.DoctorId = @DoctorId
                       AND A.IsActive = 1
                       AND A.Status = 'Scheduled'
                       AND CAST(A.StartDateTime AS DATE) = @Date
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
         @DoctorId AS DoctorId,
         CAST(@Date AS DATETIME2) AS [Date],
        @DayOfWeek AS DayOfWeek,
        DATEADD(MINUTE, S.StartMinute + (N.N * @AppointmentDurationMinutes), CAST(@Date AS DATETIME2)) AS StartDateTime,
        DATEADD(MINUTE, S.StartMinute + ((N.N + 1) * @AppointmentDurationMinutes), CAST(@Date AS DATETIME2)) AS EndDateTime,
        @AppointmentDurationMinutes AS DurationMinutes
     FROM ActiveSchedules S
            INNER JOIN NumberSeries N
     ON S.StartMinute + ((N.N + 1) * @AppointmentDurationMinutes) <= S.EndMinute
     WHERE NOT EXISTS
         (
            SELECT 1
            FROM BusyAppointments BA
            WHERE BA.StartDateTime < DATEADD(MINUTE, S.StartMinute + ((N.N + 1) * @AppointmentDurationMinutes), CAST(@Date AS DATETIME2))
       AND BA.EndDateTime > DATEADD(MINUTE, S.StartMinute + (N.N * @AppointmentDurationMinutes), CAST(@Date AS DATETIME2))
            )
     ORDER BY StartDateTime
     OPTION (MAXRECURSION 300);
END