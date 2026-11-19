using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RashePharma.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class RemoveLeadOutreachAndEmailCampaignInfrastructure : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "EmailDeliveryEvents");

            migrationBuilder.DropTable(
                name: "EmailOutboxMessages");

            migrationBuilder.DropTable(
                name: "EmailQuotaReservations");

            migrationBuilder.DropTable(
                name: "EmailSuppressions");

            migrationBuilder.DropTable(
                name: "LeadActivities");

            migrationBuilder.DropTable(
                name: "LeadImportRows");

            migrationBuilder.DropTable(
                name: "LeadOutreachResponses");

            migrationBuilder.DropTable(
                name: "EmailJobs");

            migrationBuilder.DropTable(
                name: "EmailQuotaBuckets");

            migrationBuilder.DropTable(
                name: "LeadImportBatches");

            migrationBuilder.DropTable(
                name: "LeadOutreachRecipients");

            migrationBuilder.DropTable(
                name: "ExternalLeads");

            migrationBuilder.DropTable(
                name: "LeadOutreachCampaigns");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "EmailQuotaBuckets",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DailyLimit = table.Column<int>(type: "int", nullable: false),
                    Provider = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    ReservedCount = table.Column<int>(type: "int", nullable: false),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false),
                    UpdatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    WindowEndUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    WindowStartUtc = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EmailQuotaBuckets", x => x.Id);
                    table.CheckConstraint("CK_EmailQuotaBuckets_ValidCounts", "[DailyLimit] >= 0 AND [ReservedCount] >= 0 AND [ReservedCount] <= [DailyLimit]");
                    table.CheckConstraint("CK_EmailQuotaBuckets_ValidWindow", "[WindowEndUtc] > [WindowStartUtc]");
                });

            migrationBuilder.CreateTable(
                name: "EmailSuppressions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    EmailNormalized = table.Column<string>(type: "nvarchar(320)", maxLength: 320, nullable: false),
                    LiftedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Reason = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    Source = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    SuppressionType = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EmailSuppressions", x => x.Id);
                    table.CheckConstraint("CK_EmailSuppressions_Type", "[SuppressionType] IN ('Unsubscribed', 'HardBounce', 'Complaint')");
                });

            migrationBuilder.CreateTable(
                name: "ExternalLeads",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    BusinessType = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Company = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    Country = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Email = table.Column<string>(type: "nvarchar(320)", maxLength: 320, nullable: true),
                    EmailNormalized = table.Column<string>(type: "nvarchar(320)", maxLength: 320, nullable: true),
                    HistoricalMailSentOn = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    IsSuppressed = table.Column<bool>(type: "bit", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    PermissionEvidence = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    PermissionStatus = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Phone = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: true),
                    ProductInterest = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false),
                    Source = table.Column<string>(type: "nvarchar(80)", maxLength: 80, nullable: false),
                    SuppressedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ExternalLeads", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "LeadImportBatches",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ConfirmedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DuplicateRows = table.Column<int>(type: "int", nullable: false),
                    FileName = table.Column<string>(type: "nvarchar(260)", maxLength: 260, nullable: false),
                    ImportedRows = table.Column<int>(type: "int", nullable: false),
                    InvalidRows = table.Column<int>(type: "int", nullable: false),
                    Sha256 = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                    Status = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    TotalRows = table.Column<int>(type: "int", nullable: false),
                    ValidRows = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LeadImportBatches", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "LeadOutreachCampaigns",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    AttachBrochure = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    BodyText = table.Column<string>(type: "nvarchar(max)", maxLength: 12000, nullable: false),
                    CompletedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(320)", maxLength: 320, nullable: false),
                    FailedCount = table.Column<int>(type: "int", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(160)", maxLength: 160, nullable: false),
                    QueuedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    RecipientCount = table.Column<int>(type: "int", nullable: false),
                    SentCount = table.Column<int>(type: "int", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Subject = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LeadOutreachCampaigns", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "LeadActivities",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Actor = table.Column<string>(type: "nvarchar(320)", maxLength: 320, nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Details = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false),
                    LeadId = table.Column<int>(type: "int", nullable: false),
                    Type = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LeadActivities", x => x.Id);
                    table.ForeignKey(
                        name: "FK_LeadActivities_ExternalLeads_LeadId",
                        column: x => x.LeadId,
                        principalTable: "ExternalLeads",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "LeadImportRows",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    BatchId = table.Column<int>(type: "int", nullable: false),
                    Company = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    Email = table.Column<string>(type: "nvarchar(320)", maxLength: 320, nullable: true),
                    EmailNormalized = table.Column<string>(type: "nvarchar(320)", maxLength: 320, nullable: true),
                    HistoricalMailSentOn = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    IsDuplicate = table.Column<bool>(type: "bit", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Phone = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: true),
                    ProductInterest = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    SheetName = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: false),
                    SheetRow = table.Column<int>(type: "int", nullable: false),
                    ValidationError = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LeadImportRows", x => x.Id);
                    table.ForeignKey(
                        name: "FK_LeadImportRows_LeadImportBatches_BatchId",
                        column: x => x.BatchId,
                        principalTable: "LeadImportBatches",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "LeadOutreachRecipients",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CampaignId = table.Column<int>(type: "int", nullable: false),
                    LeadId = table.Column<int>(type: "int", nullable: false),
                    AttemptCount = table.Column<int>(type: "int", nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ErrorMessage = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    ProcessingAtUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    SentAtUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Status = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LeadOutreachRecipients", x => x.Id);
                    table.ForeignKey(
                        name: "FK_LeadOutreachRecipients_ExternalLeads_LeadId",
                        column: x => x.LeadId,
                        principalTable: "ExternalLeads",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_LeadOutreachRecipients_LeadOutreachCampaigns_CampaignId",
                        column: x => x.CampaignId,
                        principalTable: "LeadOutreachCampaigns",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "EmailJobs",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    AcceptedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    AttemptCount = table.Column<int>(type: "int", nullable: false),
                    Category = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    CorrelationId = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DeliveryStatus = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    EmailType = table.Column<string>(type: "nvarchar(60)", maxLength: 60, nullable: false),
                    IdempotencyKey = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    LastErrorCode = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    LastErrorMessage = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    LeadOutreachRecipientId = table.Column<int>(type: "int", nullable: true),
                    NextAttemptAtUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    PayloadJson = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ProcessingLeaseUntilUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ProviderMessageId = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    RecipientEmail = table.Column<string>(type: "nvarchar(320)", maxLength: 320, nullable: false),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false),
                    Status = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: false),
                    TemplateKey = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    TemplateVersion = table.Column<int>(type: "int", nullable: false),
                    UpdatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EmailJobs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_EmailJobs_LeadOutreachRecipients_LeadOutreachRecipientId",
                        column: x => x.LeadOutreachRecipientId,
                        principalTable: "LeadOutreachRecipients",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "LeadOutreachResponses",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    RecipientId = table.Column<int>(type: "int", nullable: false),
                    Message = table.Column<string>(type: "nvarchar(max)", maxLength: 5000, nullable: false),
                    SubmittedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LeadOutreachResponses", x => x.Id);
                    table.ForeignKey(
                        name: "FK_LeadOutreachResponses_LeadOutreachRecipients_RecipientId",
                        column: x => x.RecipientId,
                        principalTable: "LeadOutreachRecipients",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "EmailDeliveryEvents",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    EmailJobId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    EventType = table.Column<string>(type: "nvarchar(60)", maxLength: 60, nullable: false),
                    OccurredAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Provider = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    ProviderEventId = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    RawPayloadJson = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ReceivedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EmailDeliveryEvents", x => x.Id);
                    table.ForeignKey(
                        name: "FK_EmailDeliveryEvents_EmailJobs_EmailJobId",
                        column: x => x.EmailJobId,
                        principalTable: "EmailJobs",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "EmailOutboxMessages",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    AttemptCount = table.Column<int>(type: "int", nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    EmailJobId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    EventType = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    LastError = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    LeaseUntilUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    NextAttemptAtUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    PayloadJson = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    PublishedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false),
                    SchemaVersion = table.Column<int>(type: "int", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EmailOutboxMessages", x => x.Id);
                    table.ForeignKey(
                        name: "FK_EmailOutboxMessages_EmailJobs_EmailJobId",
                        column: x => x.EmailJobId,
                        principalTable: "EmailJobs",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "EmailQuotaReservations",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    EmailJobId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    QuotaBucketId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    AttemptNumber = table.Column<int>(type: "int", nullable: false),
                    ConsumedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ReleasedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ReservedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EmailQuotaReservations", x => x.Id);
                    table.ForeignKey(
                        name: "FK_EmailQuotaReservations_EmailJobs_EmailJobId",
                        column: x => x.EmailJobId,
                        principalTable: "EmailJobs",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_EmailQuotaReservations_EmailQuotaBuckets_QuotaBucketId",
                        column: x => x.QuotaBucketId,
                        principalTable: "EmailQuotaBuckets",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_EmailDeliveryEvents_EmailJobId_OccurredAtUtc",
                table: "EmailDeliveryEvents",
                columns: new[] { "EmailJobId", "OccurredAtUtc" });

            migrationBuilder.CreateIndex(
                name: "IX_EmailDeliveryEvents_Provider_ProviderEventId",
                table: "EmailDeliveryEvents",
                columns: new[] { "Provider", "ProviderEventId" },
                unique: true,
                filter: "[ProviderEventId] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_EmailJobs_Category_Status_CreatedAtUtc",
                table: "EmailJobs",
                columns: new[] { "Category", "Status", "CreatedAtUtc" });

            migrationBuilder.CreateIndex(
                name: "IX_EmailJobs_IdempotencyKey",
                table: "EmailJobs",
                column: "IdempotencyKey",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_EmailJobs_LeadOutreachRecipientId",
                table: "EmailJobs",
                column: "LeadOutreachRecipientId",
                unique: true,
                filter: "[LeadOutreachRecipientId] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_EmailJobs_ProviderMessageId",
                table: "EmailJobs",
                column: "ProviderMessageId",
                unique: true,
                filter: "[ProviderMessageId] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_EmailJobs_Status_NextAttemptAtUtc_CreatedAtUtc",
                table: "EmailJobs",
                columns: new[] { "Status", "NextAttemptAtUtc", "CreatedAtUtc" });

            migrationBuilder.CreateIndex(
                name: "IX_EmailOutboxMessages_EmailJobId_EventType",
                table: "EmailOutboxMessages",
                columns: new[] { "EmailJobId", "EventType" },
                unique: true,
                filter: "[EventType] = 'EmailRequested'");

            migrationBuilder.CreateIndex(
                name: "IX_EmailOutboxMessages_Status_NextAttemptAtUtc_CreatedAtUtc",
                table: "EmailOutboxMessages",
                columns: new[] { "Status", "NextAttemptAtUtc", "CreatedAtUtc" });

            migrationBuilder.CreateIndex(
                name: "IX_EmailQuotaBuckets_Provider_WindowEndUtc",
                table: "EmailQuotaBuckets",
                columns: new[] { "Provider", "WindowEndUtc" });

            migrationBuilder.CreateIndex(
                name: "IX_EmailQuotaBuckets_Provider_WindowStartUtc_WindowEndUtc",
                table: "EmailQuotaBuckets",
                columns: new[] { "Provider", "WindowStartUtc", "WindowEndUtc" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_EmailQuotaReservations_EmailJobId_AttemptNumber",
                table: "EmailQuotaReservations",
                columns: new[] { "EmailJobId", "AttemptNumber" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_EmailQuotaReservations_QuotaBucketId_Status",
                table: "EmailQuotaReservations",
                columns: new[] { "QuotaBucketId", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_EmailSuppressions_EmailNormalized_SuppressionType",
                table: "EmailSuppressions",
                columns: new[] { "EmailNormalized", "SuppressionType" },
                unique: true,
                filter: "[LiftedAtUtc] IS NULL");

            migrationBuilder.CreateIndex(
                name: "IX_ExternalLeads_EmailNormalized",
                table: "ExternalLeads",
                column: "EmailNormalized",
                unique: true,
                filter: "[EmailNormalized] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_LeadActivities_LeadId_CreatedAtUtc",
                table: "LeadActivities",
                columns: new[] { "LeadId", "CreatedAtUtc" });

            migrationBuilder.CreateIndex(
                name: "IX_LeadImportBatches_Sha256_Status",
                table: "LeadImportBatches",
                columns: new[] { "Sha256", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_LeadImportRows_BatchId_SheetName_SheetRow",
                table: "LeadImportRows",
                columns: new[] { "BatchId", "SheetName", "SheetRow" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_LeadOutreachCampaigns_Status_CreatedAtUtc",
                table: "LeadOutreachCampaigns",
                columns: new[] { "Status", "CreatedAtUtc" });

            migrationBuilder.CreateIndex(
                name: "IX_LeadOutreachRecipients_CampaignId_LeadId",
                table: "LeadOutreachRecipients",
                columns: new[] { "CampaignId", "LeadId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_LeadOutreachRecipients_LeadId",
                table: "LeadOutreachRecipients",
                column: "LeadId");

            migrationBuilder.CreateIndex(
                name: "IX_LeadOutreachRecipients_Status_CreatedAtUtc",
                table: "LeadOutreachRecipients",
                columns: new[] { "Status", "CreatedAtUtc" });

            migrationBuilder.CreateIndex(
                name: "IX_LeadOutreachResponses_RecipientId",
                table: "LeadOutreachResponses",
                column: "RecipientId",
                unique: true);
        }
    }
}
