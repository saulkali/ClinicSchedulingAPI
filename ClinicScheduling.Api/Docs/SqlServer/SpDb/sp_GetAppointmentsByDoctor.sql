CREATE OR ALTER PROCEDURE dbo.sp_GetAppointmentsByDoctor
    @DoctorId UNIQUEIDENTIFIER
AS
BEGIN    

    SELECT
        A.Id,
        A.DoctorId,
        A.PatientId,
        A.StartDateTime,
        A.EndDateTime,
        A.DurationMinutes,
        A.Reason,
        A.Status,
        A.CancellationReason,
        A.IsActive,
        A.CreatedAt,
        A.ModifiedAt
    FROM Appointments A
    WHERE A.DoctorId = @DoctorId
      AND A.IsActive = 1
    ORDER BY A.StartDateTime;
END
