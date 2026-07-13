using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SGMC.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddPasswordResetTokens : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "appointments");

            migrationBuilder.EnsureSchema(
                name: "medical");

            migrationBuilder.EnsureSchema(
                name: "users");

            migrationBuilder.EnsureSchema(
                name: "Insurance");

            migrationBuilder.EnsureSchema(
                name: "system");

            migrationBuilder.CreateTable(
                name: "AvailabilityModes",
                schema: "medical",
                columns: table => new
                {
                    AvailabilityModeID = table.Column<short>(type: "smallint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    AvailabilityMode = table.Column<string>(type: "varchar(100)", unicode: false, maxLength: 100, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime", nullable: false, defaultValueSql: "(getdate())"),
                    UpdatedAt = table.Column<DateTime>(type: "datetime", nullable: true, defaultValueSql: "(getdate())"),
                    IsActive = table.Column<bool>(type: "bit", nullable: false, defaultValue: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__Availabi__A1FC32EB96F7E6BB", x => x.AvailabilityModeID);
                });

            migrationBuilder.CreateTable(
                name: "NetworkType",
                schema: "Insurance",
                columns: table => new
                {
                    NetworkTypeId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "varchar(50)", unicode: false, maxLength: 50, nullable: false),
                    Description = table.Column<string>(type: "varchar(255)", unicode: false, maxLength: 255, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime", nullable: false, defaultValueSql: "(getdate())"),
                    UpdatedAt = table.Column<DateTime>(type: "datetime", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false, defaultValue: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__NetworkT__C09029EE538D704E", x => x.NetworkTypeId);
                });

            migrationBuilder.CreateTable(
                name: "Roles",
                schema: "system",
                columns: table => new
                {
                    RoleID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    RoleName = table.Column<string>(type: "varchar(50)", unicode: false, maxLength: 50, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime", nullable: false, defaultValueSql: "(getdate())"),
                    UpdatedAt = table.Column<DateTime>(type: "datetime", nullable: true, defaultValueSql: "(getdate())"),
                    IsActive = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__Roles__8AFACE3A0AF7665C", x => x.RoleID);
                });

            migrationBuilder.CreateTable(
                name: "Specialties",
                schema: "medical",
                columns: table => new
                {
                    SpecialtyID = table.Column<short>(type: "smallint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SpecialtyName = table.Column<string>(type: "varchar(100)", unicode: false, maxLength: 100, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime", nullable: false, defaultValueSql: "(getdate())"),
                    UpdatedAt = table.Column<DateTime>(type: "datetime", nullable: true, defaultValueSql: "(getdate())"),
                    IsActive = table.Column<bool>(type: "bit", nullable: false, defaultValue: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__Specialt__D768F648D93CE967", x => x.SpecialtyID);
                });

            migrationBuilder.CreateTable(
                name: "Status",
                schema: "system",
                columns: table => new
                {
                    StatusID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    StatusName = table.Column<string>(type: "varchar(50)", unicode: false, maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__Status__C8EE2043759308C1", x => x.StatusID);
                });

            migrationBuilder.CreateTable(
                name: "InsuranceProviders",
                schema: "Insurance",
                columns: table => new
                {
                    InsuranceProviderID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "varchar(100)", unicode: false, maxLength: 100, nullable: false),
                    PhoneNumber = table.Column<string>(type: "varchar(15)", unicode: false, maxLength: 15, nullable: false),
                    Email = table.Column<string>(type: "varchar(100)", unicode: false, maxLength: 100, nullable: false),
                    Website = table.Column<string>(type: "varchar(255)", unicode: false, maxLength: 255, nullable: false),
                    Address = table.Column<string>(type: "varchar(255)", unicode: false, maxLength: 255, nullable: false),
                    City = table.Column<string>(type: "varchar(100)", unicode: false, maxLength: 100, nullable: false),
                    State = table.Column<string>(type: "varchar(100)", unicode: false, maxLength: 100, nullable: false),
                    Country = table.Column<string>(type: "varchar(100)", unicode: false, maxLength: 100, nullable: false),
                    ZipCode = table.Column<string>(type: "varchar(10)", unicode: false, maxLength: 10, nullable: false),
                    CoverageDetails = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    LogoUrl = table.Column<string>(type: "varchar(255)", unicode: false, maxLength: 255, nullable: false),
                    IsPreferred = table.Column<bool>(type: "bit", nullable: false),
                    NetworkTypeId = table.Column<int>(type: "int", nullable: false),
                    CustomerSupportContact = table.Column<string>(type: "varchar(15)", unicode: false, maxLength: 15, nullable: false),
                    AcceptedRegions = table.Column<string>(type: "varchar(255)", unicode: false, maxLength: 255, nullable: false),
                    MaxCoverageAmount = table.Column<decimal>(type: "decimal(10,2)", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime", nullable: false, defaultValueSql: "(getdate())"),
                    UpdatedAt = table.Column<DateTime>(type: "datetime", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false, defaultValue: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_InsuranceProviders", x => x.InsuranceProviderID);
                    table.ForeignKey(
                        name: "FK_InsuranceProviders_NetworkType",
                        column: x => x.NetworkTypeId,
                        principalSchema: "Insurance",
                        principalTable: "NetworkType",
                        principalColumn: "NetworkTypeId");
                });

            migrationBuilder.CreateTable(
                name: "Users",
                schema: "users",
                columns: table => new
                {
                    UserID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Email = table.Column<string>(type: "varchar(255)", unicode: false, maxLength: 255, nullable: false),
                    Password = table.Column<string>(type: "varchar(255)", unicode: false, maxLength: 255, nullable: false),
                    RoleID = table.Column<int>(type: "int", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime", nullable: false, defaultValueSql: "(getdate())"),
                    UpdatedAt = table.Column<DateTime>(type: "datetime", nullable: true, defaultValueSql: "(getdate())"),
                    IsActive = table.Column<bool>(type: "bit", nullable: false, defaultValue: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__Users__1788CCACD6E62048", x => x.UserID);
                    table.ForeignKey(
                        name: "FK__Users__RoleID__4316F928",
                        column: x => x.RoleID,
                        principalSchema: "system",
                        principalTable: "Roles",
                        principalColumn: "RoleID");
                });

            migrationBuilder.CreateTable(
                name: "Notifications",
                schema: "system",
                columns: table => new
                {
                    NotificationID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserID = table.Column<int>(type: "int", nullable: false),
                    SentAt = table.Column<DateTime>(type: "datetime", nullable: true, defaultValueSql: "(getdate())"),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    RecipientId = table.Column<int>(type: "int", nullable: false),
                    Title = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsRead = table.Column<bool>(type: "bit", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__Notifica__20CF2E327D8F016B", x => x.NotificationID);
                    table.ForeignKey(
                        name: "FK__Notificat__UserI__619B8048",
                        column: x => x.UserID,
                        principalSchema: "users",
                        principalTable: "Users",
                        principalColumn: "UserID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PasswordResetTokens",
                schema: "users",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<int>(type: "int", nullable: false),
                    Token = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ExpiresAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsUsed = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PasswordResetTokens", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PasswordResetTokens_Users_UserId",
                        column: x => x.UserId,
                        principalSchema: "users",
                        principalTable: "Users",
                        principalColumn: "UserID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Persons",
                schema: "users",
                columns: table => new
                {
                    PersonID = table.Column<int>(type: "int", nullable: false),
                    FirstName = table.Column<string>(type: "varchar(40)", unicode: false, maxLength: 40, nullable: false),
                    LastName = table.Column<string>(type: "varchar(40)", unicode: false, maxLength: 40, nullable: false),
                    DateOfBirth = table.Column<DateOnly>(type: "date", nullable: true),
                    IdentificationNumber = table.Column<string>(type: "varchar(25)", unicode: false, maxLength: 25, nullable: false),
                    Gender = table.Column<string>(type: "char(1)", unicode: false, fixedLength: true, maxLength: 1, nullable: false),
                    UserId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__Persons__AA2FFB85C91EBDF7", x => x.PersonID);
                    table.ForeignKey(
                        name: "FK_Persons_Users",
                        column: x => x.PersonID,
                        principalSchema: "users",
                        principalTable: "Users",
                        principalColumn: "UserID");
                });

            migrationBuilder.CreateTable(
                name: "Doctors",
                schema: "users",
                columns: table => new
                {
                    DoctorID = table.Column<int>(type: "int", nullable: false),
                    SpecialtyID = table.Column<short>(type: "smallint", nullable: false),
                    LicenseNumber = table.Column<string>(type: "varchar(50)", unicode: false, maxLength: 50, nullable: false),
                    PhoneNumber = table.Column<string>(type: "varchar(15)", unicode: false, maxLength: 15, nullable: false),
                    YearsOfExperience = table.Column<int>(type: "int", nullable: false),
                    Education = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Bio = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ConsultationFee = table.Column<decimal>(type: "decimal(10,2)", nullable: true),
                    ClinicAddress = table.Column<string>(type: "varchar(255)", unicode: false, maxLength: 255, nullable: false),
                    AvailabilityModeId = table.Column<short>(type: "smallint", nullable: true),
                    LicenseExpirationDate = table.Column<DateOnly>(type: "date", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime", nullable: false, defaultValueSql: "(getdate())"),
                    UpdatedAt = table.Column<DateTime>(type: "datetime", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false, defaultValue: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__Doctors__2DC00EDFCF31D9E9", x => x.DoctorID);
                    table.ForeignKey(
                        name: "FK_Doctors_AvailabilityMode",
                        column: x => x.AvailabilityModeId,
                        principalSchema: "medical",
                        principalTable: "AvailabilityModes",
                        principalColumn: "AvailabilityModeID");
                    table.ForeignKey(
                        name: "FK_Doctors_Person",
                        column: x => x.DoctorID,
                        principalSchema: "users",
                        principalTable: "Persons",
                        principalColumn: "PersonID");
                    table.ForeignKey(
                        name: "FK_Doctors_Speciality",
                        column: x => x.SpecialtyID,
                        principalSchema: "medical",
                        principalTable: "Specialties",
                        principalColumn: "SpecialtyID");
                });

            migrationBuilder.CreateTable(
                name: "Employees",
                schema: "users",
                columns: table => new
                {
                    EmployeeId = table.Column<int>(type: "int", nullable: false),
                    PhoneNumber = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    JobTitle = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime", nullable: true, defaultValueSql: "(getdate())"),
                    UpdatedAt = table.Column<DateTime>(type: "datetime", nullable: true, defaultValueSql: "(getdate())"),
                    IsActive = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__Employee__7AD04F11D75E95E5", x => x.EmployeeId);
                    table.ForeignKey(
                        name: "FK_Employees_Person",
                        column: x => x.EmployeeId,
                        principalSchema: "users",
                        principalTable: "Persons",
                        principalColumn: "PersonID");
                });

            migrationBuilder.CreateTable(
                name: "Patients",
                schema: "users",
                columns: table => new
                {
                    PatientID = table.Column<int>(type: "int", nullable: false),
                    Gender = table.Column<string>(type: "char(1)", unicode: false, fixedLength: true, maxLength: 1, nullable: false),
                    PhoneNumber = table.Column<string>(type: "varchar(15)", unicode: false, maxLength: 15, nullable: false),
                    Address = table.Column<string>(type: "varchar(255)", unicode: false, maxLength: 255, nullable: false),
                    EmergencyContactName = table.Column<string>(type: "varchar(100)", unicode: false, maxLength: 100, nullable: false),
                    EmergencyContactPhone = table.Column<string>(type: "varchar(15)", unicode: false, maxLength: 15, nullable: false),
                    BloodType = table.Column<string>(type: "char(2)", unicode: false, fixedLength: true, maxLength: 2, nullable: false),
                    Allergies = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    InsuranceProviderID = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime", nullable: false, defaultValueSql: "(getdate())"),
                    UpdatedAt = table.Column<DateTime>(type: "datetime", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false, defaultValue: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__Patients__970EC346B158B7FB", x => x.PatientID);
                    table.ForeignKey(
                        name: "FK_Patients_InsuranceProvider",
                        column: x => x.InsuranceProviderID,
                        principalSchema: "Insurance",
                        principalTable: "InsuranceProviders",
                        principalColumn: "InsuranceProviderID");
                    table.ForeignKey(
                        name: "FK_Patients_Person",
                        column: x => x.PatientID,
                        principalSchema: "users",
                        principalTable: "Persons",
                        principalColumn: "PersonID");
                });

            migrationBuilder.CreateTable(
                name: "DoctorAvailability",
                schema: "appointments",
                columns: table => new
                {
                    AvailabilityID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    DoctorID = table.Column<int>(type: "int", nullable: false),
                    AvailableDate = table.Column<DateOnly>(type: "date", nullable: false),
                    StartTime = table.Column<TimeOnly>(type: "time", nullable: false),
                    EndTime = table.Column<TimeOnly>(type: "time", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__DoctorAv__DA397991EEC28676", x => x.AvailabilityID);
                    table.ForeignKey(
                        name: "FK__DoctorAva__Docto__5535A963",
                        column: x => x.DoctorID,
                        principalSchema: "users",
                        principalTable: "Doctors",
                        principalColumn: "DoctorID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Appointments",
                schema: "appointments",
                columns: table => new
                {
                    AppointmentID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PatientID = table.Column<int>(type: "int", nullable: false),
                    DoctorID = table.Column<int>(type: "int", nullable: false),
                    AppointmentDate = table.Column<DateTime>(type: "datetime", nullable: false),
                    StatusID = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime", nullable: false, defaultValueSql: "(getdate())"),
                    UpdatedAt = table.Column<DateTime>(type: "datetime", nullable: true, defaultValueSql: "(getdate())")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__Appointm__8ECDFCA291B4E736", x => x.AppointmentID);
                    table.ForeignKey(
                        name: "FK_Appointments_Doctor",
                        column: x => x.DoctorID,
                        principalSchema: "users",
                        principalTable: "Doctors",
                        principalColumn: "DoctorID");
                    table.ForeignKey(
                        name: "FK_Appointments_Patient",
                        column: x => x.PatientID,
                        principalSchema: "users",
                        principalTable: "Patients",
                        principalColumn: "PatientID");
                    table.ForeignKey(
                        name: "FK_Appointments_Status",
                        column: x => x.StatusID,
                        principalSchema: "system",
                        principalTable: "Status",
                        principalColumn: "StatusID");
                });

            migrationBuilder.CreateTable(
                name: "MedicalRecords",
                schema: "medical",
                columns: table => new
                {
                    RecordID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PatientID = table.Column<int>(type: "int", nullable: false),
                    DoctorID = table.Column<int>(type: "int", nullable: false),
                    Diagnosis = table.Column<string>(type: "text", nullable: false),
                    Treatment = table.Column<string>(type: "text", nullable: false),
                    DateOfVisit = table.Column<DateTime>(type: "datetime", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime", nullable: false, defaultValueSql: "(getdate())"),
                    UpdatedAt = table.Column<DateTime>(type: "datetime", nullable: true, defaultValueSql: "(getdate())"),
                    MedicalRecordId = table.Column<int>(type: "int", nullable: false),
                    Id = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__MedicalR__FBDF78C96E650FFC", x => x.RecordID);
                    table.ForeignKey(
                        name: "FK_MedicalRecords_Doctor",
                        column: x => x.DoctorID,
                        principalSchema: "users",
                        principalTable: "Doctors",
                        principalColumn: "DoctorID");
                    table.ForeignKey(
                        name: "FK_MedicalRecords_Patient",
                        column: x => x.PatientID,
                        principalSchema: "users",
                        principalTable: "Patients",
                        principalColumn: "PatientID");
                });

            migrationBuilder.CreateIndex(
                name: "IX_Appointments_DoctorID",
                schema: "appointments",
                table: "Appointments",
                column: "DoctorID");

            migrationBuilder.CreateIndex(
                name: "IX_Appointments_PatientID",
                schema: "appointments",
                table: "Appointments",
                column: "PatientID");

            migrationBuilder.CreateIndex(
                name: "IX_Appointments_StatusID",
                schema: "appointments",
                table: "Appointments",
                column: "StatusID");

            migrationBuilder.CreateIndex(
                name: "UQ__Availabi__8598C8508EE645A1",
                schema: "medical",
                table: "AvailabilityModes",
                column: "AvailabilityMode",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_DoctorAvailability_DoctorID",
                schema: "appointments",
                table: "DoctorAvailability",
                column: "DoctorID");

            migrationBuilder.CreateIndex(
                name: "IX_Doctors_AvailabilityModeId",
                schema: "users",
                table: "Doctors",
                column: "AvailabilityModeId");

            migrationBuilder.CreateIndex(
                name: "IX_Doctors_SpecialtyID",
                schema: "users",
                table: "Doctors",
                column: "SpecialtyID");

            migrationBuilder.CreateIndex(
                name: "IX_InsuranceProviders_NetworkTypeId",
                schema: "Insurance",
                table: "InsuranceProviders",
                column: "NetworkTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_MedicalRecords_DoctorID",
                schema: "medical",
                table: "MedicalRecords",
                column: "DoctorID");

            migrationBuilder.CreateIndex(
                name: "IX_MedicalRecords_PatientID",
                schema: "medical",
                table: "MedicalRecords",
                column: "PatientID");

            migrationBuilder.CreateIndex(
                name: "IX_Notifications_UserID",
                schema: "system",
                table: "Notifications",
                column: "UserID");

            migrationBuilder.CreateIndex(
                name: "IX_PasswordResetTokens_UserId",
                schema: "users",
                table: "PasswordResetTokens",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_Patients_InsuranceProviderID",
                schema: "users",
                table: "Patients",
                column: "InsuranceProviderID");

            migrationBuilder.CreateIndex(
                name: "UQ__Roles__8A2B61603EE5AFD9",
                schema: "system",
                table: "Roles",
                column: "RoleName",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "UQ__Specialt__7DCA574800EB0679",
                schema: "medical",
                table: "Specialties",
                column: "SpecialtyName",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "UQ__Status__05E7698AD7258224",
                schema: "system",
                table: "Status",
                column: "StatusName",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Users_RoleID",
                schema: "users",
                table: "Users",
                column: "RoleID");

            migrationBuilder.CreateIndex(
                name: "UQ__Users__A9D10534B973B1BC",
                schema: "users",
                table: "Users",
                column: "Email",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Appointments",
                schema: "appointments");

            migrationBuilder.DropTable(
                name: "DoctorAvailability",
                schema: "appointments");

            migrationBuilder.DropTable(
                name: "Employees",
                schema: "users");

            migrationBuilder.DropTable(
                name: "MedicalRecords",
                schema: "medical");

            migrationBuilder.DropTable(
                name: "Notifications",
                schema: "system");

            migrationBuilder.DropTable(
                name: "PasswordResetTokens",
                schema: "users");

            migrationBuilder.DropTable(
                name: "Status",
                schema: "system");

            migrationBuilder.DropTable(
                name: "Doctors",
                schema: "users");

            migrationBuilder.DropTable(
                name: "Patients",
                schema: "users");

            migrationBuilder.DropTable(
                name: "AvailabilityModes",
                schema: "medical");

            migrationBuilder.DropTable(
                name: "Specialties",
                schema: "medical");

            migrationBuilder.DropTable(
                name: "InsuranceProviders",
                schema: "Insurance");

            migrationBuilder.DropTable(
                name: "Persons",
                schema: "users");

            migrationBuilder.DropTable(
                name: "NetworkType",
                schema: "Insurance");

            migrationBuilder.DropTable(
                name: "Users",
                schema: "users");

            migrationBuilder.DropTable(
                name: "Roles",
                schema: "system");
        }
    }
}
