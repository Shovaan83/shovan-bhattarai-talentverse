using Dapper;
using TalentVerse.WebAPI.Data;
using TalentVerse.WebAPI.Data.Entities;
using TalentVerse.WebAPI.Data.Enums;
using TalentVerse.WebAPI.DTO.Appointments;
using TalentVerse.WebAPI.Interfaces;

namespace TalentVerse.WebAPI.Repositories
{
    public class AppointmentRepository : IAppointmentRepository
    {
        private readonly DapperContext _context;

        public AppointmentRepository(DapperContext context)
        {
            _context = context;
        }

        public async Task<Appointment?> CreateAsync(Appointment appointment)
        {
            using var connection = _context.CreateConnection();
            connection.Open();
            using var transaction = connection.BeginTransaction();

            try
            {
                var sql = @"
                    INSERT INTO ""Appointments""
                        (""ProposalId"", ""CreatedByUserId"", ""MeetingTime"", ""Duration"",
                         ""Description"", ""MeetingLink"", ""Status"", ""GoogleCalendarEventId"",
                         ""CreatedAt"", ""UpdatedAt"")
                    VALUES
                        (@ProposalId, @CreatedByUserId, @MeetingTime, @Duration,
                         @Description, @MeetingLink, @Status, @GoogleCalendarEventId,
                         @CreatedAt, @UpdatedAt)
                    RETURNING ""AppointmentId""";

                appointment.AppointmentId = await connection.QuerySingleAsync<int>(sql, new
                {
                    appointment.ProposalId,
                    appointment.CreatedByUserId,
                    appointment.MeetingTime,
                    appointment.Duration,
                    appointment.Description,
                    appointment.MeetingLink,
                    Status = (int)appointment.Status,
                    appointment.GoogleCalendarEventId,
                    appointment.CreatedAt,
                    appointment.UpdatedAt
                }, transaction);

                transaction.Commit();
                return appointment;
            }
            catch
            {
                transaction.Rollback();
                throw;
            }
        }

        public async Task<Appointment?> GetByIdAsync(int appointmentId)
        {
            using var connection = _context.CreateConnection();
            var sql = @"
                SELECT ""AppointmentId"", ""ProposalId"", ""CreatedByUserId"", ""MeetingTime"",
                       ""Duration"", ""Description"", ""MeetingLink"", ""Status"",
                       ""GoogleCalendarEventId"", ""CreatedAt"", ""UpdatedAt""
                FROM   ""Appointments""
                WHERE  ""AppointmentId"" = @AppointmentId";

            var result = await connection.QuerySingleOrDefaultAsync<dynamic>(sql, new { AppointmentId = appointmentId });
            if (result == null) return null;

            return new Appointment
            {
                AppointmentId = result.AppointmentId,
                ProposalId = result.ProposalId,
                CreatedByUserId = result.CreatedByUserId,
                MeetingTime = DateTime.SpecifyKind(result.MeetingTime, DateTimeKind.Utc),
                Duration = result.Duration,
                Description = result.Description,
                MeetingLink = result.MeetingLink,
                Status = (AppointmentStatus)result.Status,
                GoogleCalendarEventId = result.GoogleCalendarEventId,
                CreatedAt = DateTime.SpecifyKind(result.CreatedAt, DateTimeKind.Utc),
                UpdatedAt = DateTime.SpecifyKind(result.UpdatedAt, DateTimeKind.Utc)
            };
        }

        public async Task<AppointmentDto?> GetDtoByIdAsync(int appointmentId)
        {
            using var connection = _context.CreateConnection();
            var sql = @"
                SELECT a.""AppointmentId"",
                       a.""ProposalId"",
                       a.""CreatedByUserId"",
                       u.""UserName"" AS ""CreatedByUsername"",
                       a.""MeetingTime"",
                       a.""Duration"",
                       a.""Description"",
                       a.""MeetingLink"",
                       a.""Status"",
                       a.""CreatedAt"",
                       a.""UpdatedAt""
                FROM   ""Appointments"" a
                INNER JOIN ""AspNetUsers"" u ON u.""Id"" = a.""CreatedByUserId""
                WHERE  a.""AppointmentId"" = @AppointmentId";

            var row = await connection.QuerySingleOrDefaultAsync<AppointmentQueryResult>(sql, new { AppointmentId = appointmentId });
            return row == null ? null : MapToDto(row);
        }

        public async Task<List<AppointmentDto>> GetByProposalIdAsync(int proposalId)
        {
            using var connection = _context.CreateConnection();
            var sql = @"
                SELECT a.""AppointmentId"",
                       a.""ProposalId"",
                       a.""CreatedByUserId"",
                       u.""UserName"" AS ""CreatedByUsername"",
                       a.""MeetingTime"",
                       a.""Duration"",
                       a.""Description"",
                       a.""MeetingLink"",
                       a.""Status"",
                       a.""CreatedAt"",
                       a.""UpdatedAt""
                FROM   ""Appointments"" a
                INNER JOIN ""AspNetUsers"" u ON u.""Id"" = a.""CreatedByUserId""
                WHERE  a.""ProposalId"" = @ProposalId
                ORDER  BY a.""CreatedAt"" DESC";

            var rows = await connection.QueryAsync<AppointmentQueryResult>(sql, new { ProposalId = proposalId });
            return rows.Select(MapToDto).ToList();
        }

        public async Task<bool> UpdateAsync(Appointment appointment)
        {
            using var connection = _context.CreateConnection();
            connection.Open();
            using var transaction = connection.BeginTransaction();

            try
            {
                var sql = @"
                    UPDATE ""Appointments""
                    SET    ""MeetingTime""          = @MeetingTime,
                           ""Duration""             = @Duration,
                           ""Description""          = @Description,
                           ""MeetingLink""          = @MeetingLink,
                           ""Status""               = @Status,
                           ""GoogleCalendarEventId"" = @GoogleCalendarEventId,
                           ""UpdatedAt""            = @UpdatedAt
                    WHERE  ""AppointmentId"" = @AppointmentId";

                var affected = await connection.ExecuteAsync(sql, new
                {
                    appointment.MeetingTime,
                    appointment.Duration,
                    appointment.Description,
                    appointment.MeetingLink,
                    Status = (int)appointment.Status,
                    appointment.GoogleCalendarEventId,
                    appointment.UpdatedAt,
                    appointment.AppointmentId
                }, transaction);

                transaction.Commit();
                return affected > 0;
            }
            catch
            {
                transaction.Rollback();
                throw;
            }
        }

        // ---- Internal mapping types ---- //

        private static AppointmentDto MapToDto(AppointmentQueryResult r) => new()
        {
            AppointmentId = r.AppointmentId,
            ProposalId = r.ProposalId,
            CreatedByUserId = r.CreatedByUserId,
            CreatedByUsername = r.CreatedByUsername,
            MeetingTime = DateTime.SpecifyKind(r.MeetingTime, DateTimeKind.Utc),
            Duration = r.Duration,
            Description = r.Description,
            MeetingLink = r.MeetingLink,
            Status = ((AppointmentStatus)r.Status).ToString(),
            CreatedAt = DateTime.SpecifyKind(r.CreatedAt, DateTimeKind.Utc),
            UpdatedAt = DateTime.SpecifyKind(r.UpdatedAt, DateTimeKind.Utc)
        };

        private sealed class AppointmentQueryResult
        {
            public int AppointmentId { get; set; }
            public int ProposalId { get; set; }
            public string CreatedByUserId { get; set; } = string.Empty;
            public string CreatedByUsername { get; set; } = string.Empty;
            public DateTime MeetingTime { get; set; }
            public int Duration { get; set; }
            public string? Description { get; set; }
            public string? MeetingLink { get; set; }
            public int Status { get; set; }
            public DateTime CreatedAt { get; set; }
            public DateTime UpdatedAt { get; set; }
        }
    }
}
