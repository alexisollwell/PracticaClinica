-- =====================================================================
-- Base de Datos: ClinicaMedicaDb
-- Sistema de Gestión de Citas Médicas y Consultorios
-- Motor: Microsoft SQL Server
-- =====================================================================

IF NOT EXISTS (SELECT name FROM sys.databases WHERE name = 'ClinicaMedicaDb')
BEGIN
    CREATE DATABASE ClinicaMedicaDb;
END;
GO

USE ClinicaMedicaDb;
GO

-- 1. CREACIÓN DE TABLAS
-- =====================================================================

-- Tabla de Usuarios (Pacientes, Doctores, Administradores)
IF OBJECT_ID('Appointments', 'U') IS NOT NULL DROP TABLE Appointments;
IF OBJECT_ID('Doctors', 'U') IS NOT NULL DROP TABLE Doctors;
IF OBJECT_ID('Specialties', 'U') IS NOT NULL DROP TABLE Specialties;
IF OBJECT_ID('ApiKeys', 'U') IS NOT NULL DROP TABLE ApiKeys;
IF OBJECT_ID('Users', 'U') IS NOT NULL DROP TABLE Users;

CREATE TABLE Users (
    UserId INT IDENTITY(1,1) PRIMARY KEY,
    Username NVARCHAR(50) NOT NULL UNIQUE,
    FullName NVARCHAR(100) NOT NULL,
    Email NVARCHAR(100) NOT NULL UNIQUE,
    PasswordHash NVARCHAR(255) NOT NULL,
    Role NVARCHAR(20) NOT NULL DEFAULT 'Patient', -- 'Patient', 'Doctor', 'Admin'
    CreatedAt DATETIME DEFAULT GETDATE()
);

-- Tabla de API Keys (Para integraciones M2M como bots de WhatsApp / SMS)
CREATE TABLE ApiKeys (
    ApiKeyId INT IDENTITY(1,1) PRIMARY KEY,
    KeyName NVARCHAR(50) NOT NULL,
    KeyValue UNIQUEIDENTIFIER NOT NULL UNIQUE DEFAULT NEWID(),
    IsActive BIT NOT NULL DEFAULT 1,
    CreatedAt DATETIME DEFAULT GETDATE()
);

-- Tabla de Especialidades Médicas
CREATE TABLE Specialties (
    SpecialtyId INT IDENTITY(1,1) PRIMARY KEY,
    Name NVARCHAR(100) NOT NULL UNIQUE,
    Description NVARCHAR(255),
    ConsultationFee DECIMAL(18,2) NOT NULL DEFAULT 500.00,
    IsActive BIT NOT NULL DEFAULT 1
);

-- Tabla de Médicos
CREATE TABLE Doctors (
    DoctorId INT IDENTITY(1,1) PRIMARY KEY,
    UserId INT NOT NULL FOREIGN KEY REFERENCES Users(UserId),
    SpecialtyId INT NOT NULL FOREIGN KEY REFERENCES Specialties(SpecialtyId),
    ProfessionalLicense NVARCHAR(50) NOT NULL UNIQUE, -- Cédula Profesional
    ConsultationRoom NVARCHAR(20) NOT NULL,          -- Consultorio (ej: 'C-204')
    WorkStartTime TIME NOT NULL DEFAULT '08:00:00',
    WorkEndTime TIME NOT NULL DEFAULT '18:00:00',
    SlotDurationMinutes INT NOT NULL DEFAULT 30,     -- Duración estándar de cada cita
    IsActive BIT NOT NULL DEFAULT 1
);

-- Tabla de Citas Médicas
CREATE TABLE Appointments (
    AppointmentId INT IDENTITY(1,1) PRIMARY KEY,
    PatientUserId INT NOT NULL FOREIGN KEY REFERENCES Users(UserId),
    DoctorId INT NOT NULL FOREIGN KEY REFERENCES Doctors(DoctorId),
    AppointmentDate DATE NOT NULL,
    StartTime TIME NOT NULL,
    EndTime TIME NOT NULL,
    Reason NVARCHAR(255) NOT NULL,
    Notes NVARCHAR(1000) NULL,
    Status NVARCHAR(20) NOT NULL DEFAULT 'Pending', -- 'Pending', 'Confirmed', 'Cancelled', 'Completed'
    ReminderSent BIT NOT NULL DEFAULT 0,
    CreatedAt DATETIME DEFAULT GETDATE()
);
GO

-- 2. INSERCIÓN DE DATOS INICIALES (SEEDING)
-- =====================================================================

-- Usuarios de prueba
INSERT INTO Users (Username, FullName, Email, PasswordHash, Role) VALUES
('admin_clinica', 'Lic. Sofia Ramos', 'admin@clinica.com', 'admin123', 'Admin'),
('dr_garcia', 'Dr. Carlos Garcia', 'carlos.garcia@clinica.com', 'doctor123', 'Doctor'),
('dra_lopez', 'Dra. Maria Lopez', 'maria.lopez@clinica.com', 'doctor123', 'Doctor'),
('paciente_juan', 'Juan Perez', 'juan.perez@gmail.com', 'paciente123', 'Patient'),
('paciente_ana', 'Ana Torres', 'ana.torres@gmail.com', 'paciente123', 'Patient');

-- API Keys para servicios externos
INSERT INTO ApiKeys (KeyName, KeyValue, IsActive) VALUES
('Bot_WhatsApp_Recordatorios', 'A1B2C3D4-E5F6-7890-ABCD-EF1234567890', 1),
('Sistema_Facturacion_Externo', 'F9E8D7C6-B5A4-3210-FEDC-BA0987654321', 1);

-- Especialidades
INSERT INTO Specialties (Name, Description, ConsultationFee, IsActive) VALUES
('Medicina General', 'Atención médica primaria y preventiva', 400.00, 1),
('Cardiología', 'Diagnóstico y tratamiento de enfermedades cardíacas', 900.00, 1),
('Pediatría', 'Atención integral para recién nacidos, niños y adolescentes', 700.00, 1),
('Dermatología', 'Cuidado de la piel, cabello y uñas', 850.00, 1);

-- Médicos asignados
INSERT INTO Doctors (UserId, SpecialtyId, ProfessionalLicense, ConsultationRoom, WorkStartTime, WorkEndTime, SlotDurationMinutes, IsActive) VALUES
(2, 1, 'CED-98745612', 'Cons-101', '08:00:00', '16:00:00', 30, 1), -- Dr. Carlos Garcia (Medicina General)
(3, 2, 'CED-45612378', 'Cons-205', '10:00:00', '18:00:00', 45, 1); -- Dra. Maria Lopez (Cardiología)
GO

-- 3. PROCEDIMIENTOS ALMACENADOS (STORED PROCEDURES)
-- =====================================================================

-- SP 1: Obtener catálogo público de especialidades activas
CREATE OR ALTER PROCEDURE sp_GetActiveSpecialties
AS
BEGIN
    SET NOCOUNT ON;
    SELECT SpecialtyId, Name, Description, ConsultationFee
    FROM Specialties
    WHERE IsActive = 1
    ORDER BY Name ASC;
END;
GO

-- SP 2: Obtener catálogo público de médicos con sus especialidades
CREATE OR ALTER PROCEDURE sp_GetActiveDoctors
    @SpecialtyId INT = NULL
AS
BEGIN
    SET NOCOUNT ON;
    SELECT 
        d.DoctorId,
        d.UserId,
        u.FullName AS DoctorName,
        s.SpecialtyId,
        s.Name AS SpecialtyName,
        d.ProfessionalLicense,
        d.ConsultationRoom,
        d.WorkStartTime,
        d.WorkEndTime,
        d.SlotDurationMinutes,
        s.ConsultationFee
    FROM Doctors d
    INNER JOIN Users u ON d.UserId = u.UserId
    INNER JOIN Specialties s ON d.SpecialtyId = s.SpecialtyId
    WHERE d.IsActive = 1
      AND (@SpecialtyId IS NULL OR d.SpecialtyId = @SpecialtyId)
    ORDER BY u.FullName ASC;
END;
GO

-- SP 3: Validar disponibilidad de horario para un médico (Prevenir empalmes)
CREATE OR ALTER PROCEDURE sp_CheckDoctorSlotCollision
    @DoctorId INT,
    @AppointmentDate DATE,
    @StartTime TIME,
    @EndTime TIME
AS
BEGIN
    SET NOCOUNT ON;
    SELECT COUNT(1) AS CollisionsCount
    FROM Appointments
    WHERE DoctorId = @DoctorId
      AND AppointmentDate = @AppointmentDate
      AND Status NOT IN ('Cancelled')
      AND (
          (@StartTime >= StartTime AND @StartTime < EndTime) OR
          (@EndTime > StartTime AND @EndTime <= EndTime) OR
          (@StartTime <= StartTime AND @EndTime >= EndTime)
      );
END;
GO

-- SP 4: Registrar nueva cita médica
CREATE OR ALTER PROCEDURE sp_CreateAppointment
    @PatientUserId INT,
    @DoctorId INT,
    @AppointmentDate DATE,
    @StartTime TIME,
    @EndTime TIME,
    @Reason NVARCHAR(255),
    @AppointmentId INT OUTPUT
AS
BEGIN
    SET NOCOUNT ON;

    INSERT INTO Appointments (PatientUserId, DoctorId, AppointmentDate, StartTime, EndTime, Reason, Status, ReminderSent, CreatedAt)
    VALUES (@PatientUserId, @DoctorId, @AppointmentDate, @StartTime, @EndTime, @Reason, 'Pending', 0, GETDATE());

    SET @AppointmentId = SCOPE_IDENTITY();
END;
GO

-- SP 5: Obtener citas del paciente autenticado ("Mis Citas")
CREATE OR ALTER PROCEDURE sp_GetMyAppointments
    @PatientUserId INT
AS
BEGIN
    SET NOCOUNT ON;
    SELECT 
        a.AppointmentId,
        a.AppointmentDate,
        a.StartTime,
        a.EndTime,
        a.Reason,
        a.Notes,
        a.Status,
        uDoc.FullName AS DoctorName,
        s.Name AS SpecialtyName,
        d.ConsultationRoom,
        s.ConsultationFee
    FROM Appointments a
    INNER JOIN Doctors d ON a.DoctorId = d.DoctorId
    INNER JOIN Users uDoc ON d.UserId = uDoc.UserId
    INNER JOIN Specialties s ON d.SpecialtyId = s.SpecialtyId
    WHERE a.PatientUserId = @PatientUserId
    ORDER BY a.AppointmentDate DESC, a.StartTime DESC;
END;
GO

-- SP 6: Obtener citas globales o por Doctor (para Staff Médico y Admin)
CREATE OR ALTER PROCEDURE sp_GetAllAppointments
    @DoctorUserId INT = NULL,
    @Date DATE = NULL
AS
BEGIN
    SET NOCOUNT ON;
    SELECT 
        a.AppointmentId,
        a.AppointmentDate,
        a.StartTime,
        a.EndTime,
        a.Reason,
        a.Notes,
        a.Status,
        uPat.FullName AS PatientName,
        uPat.Email AS PatientEmail,
        uDoc.FullName AS DoctorName,
        s.Name AS SpecialtyName,
        d.ConsultationRoom
    FROM Appointments a
    INNER JOIN Users uPat ON a.PatientUserId = uPat.UserId
    INNER JOIN Doctors d ON a.DoctorId = d.DoctorId
    INNER JOIN Users uDoc ON d.UserId = uDoc.UserId
    INNER JOIN Specialties s ON d.SpecialtyId = s.SpecialtyId
    WHERE (@DoctorUserId IS NULL OR d.UserId = @DoctorUserId)
      AND (@Date IS NULL OR a.AppointmentDate = @Date)
    ORDER BY a.AppointmentDate ASC, a.StartTime ASC;
END;
GO

-- SP 7: Actualizar estado de una cita médica (Cancelar, Confirmar, Completar)
CREATE OR ALTER PROCEDURE sp_UpdateAppointmentStatus
    @AppointmentId INT,
    @NewStatus NVARCHAR(20),
    @Notes NVARCHAR(1000) = NULL
AS
BEGIN
    SET NOCOUNT ON;

    UPDATE Appointments
    SET Status = @NewStatus,
        Notes = ISNULL(@Notes, Notes)
    WHERE AppointmentId = @AppointmentId;

    SELECT @@ROWCOUNT AS RowsAffected;
END;
GO

-- SP 8: Obtener citas para bot externo de WhatsApp/SMS (Requiere API Key)
CREATE OR ALTER PROCEDURE sp_GetUpcomingAppointmentsForReminder
    @TargetDate DATE
AS
BEGIN
    SET NOCOUNT ON;
    SELECT 
        a.AppointmentId,
        uPat.FullName AS PatientName,
        uPat.Email AS PatientEmail,
        uDoc.FullName AS DoctorName,
        s.Name AS SpecialtyName,
        d.ConsultationRoom,
        a.AppointmentDate,
        a.StartTime
    FROM Appointments a
    INNER JOIN Users uPat ON a.PatientUserId = uPat.UserId
    INNER JOIN Doctors d ON a.DoctorId = d.DoctorId
    INNER JOIN Users uDoc ON d.UserId = uDoc.UserId
    INNER JOIN Specialties s ON d.SpecialtyId = s.SpecialtyId
    WHERE a.AppointmentDate = @TargetDate
      AND a.Status = 'Pending'
      AND a.ReminderSent = 0;
END;
GO

-- SP 9: Marcar recordatorio como enviado (Webhook del bot de mensajería)
CREATE OR ALTER PROCEDURE sp_MarkReminderAsSent
    @AppointmentId INT
AS
BEGIN
    SET NOCOUNT ON;
    UPDATE Appointments
    SET ReminderSent = 1
    WHERE AppointmentId = @AppointmentId;

    SELECT @@ROWCOUNT AS RowsAffected;
END;
GO

