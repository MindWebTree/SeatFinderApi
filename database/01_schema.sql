-- =====================================================================
-- Counselling App - Database Schema (MySQL 8.0+)
--
-- Naming: PascalCase table/column names (Id, Name, FullName, ...).
-- Constraint/index names stay lowercase snake_case (uq_, fk_, ix_ prefixes).
--
-- Key strategy:
--   - Roles, Users: GUID-only primary key (CHAR(36)). No int Id at all.
--     Any column relating TO these tables is also CHAR(36).
--   - Users <-> Roles is now MANY-TO-MANY via the UserRole junction table
--     (a user can hold more than one role) instead of a single RoleId on Users.
--   - Every other table: DUAL key -
--       Id    INT AUTO_INCREMENT PRIMARY KEY   (fast internal joins/FKs)
--       Guid  CHAR(36) UNIQUE DEFAULT (UUID())  (safe public-facing id)
--     Relations BETWEEN these "regular" tables use the INT Id.
--   - Every table carries audit columns: IsDeleted, CreatedOn, UpdatedOn.
-- Run this first.
-- =====================================================================

CREATE DATABASE IF NOT EXISTS counselling_app
    CHARACTER SET utf8mb4
    COLLATE utf8mb4_unicode_ci;

USE counselling_app;

-- ---------------------------------------------------------------------
-- Roles (GUID only)
-- ---------------------------------------------------------------------
CREATE TABLE IF NOT EXISTS Roles (
    Id         CHAR(36)     NOT NULL PRIMARY KEY,
    Name       VARCHAR(50)  NOT NULL,
    IsDeleted  TINYINT(1)   NOT NULL DEFAULT 0,
    CreatedOn  DATETIME     NOT NULL DEFAULT CURRENT_TIMESTAMP,
    UpdatedOn  DATETIME     NULL ON UPDATE CURRENT_TIMESTAMP,
    CONSTRAINT uq_roles_name UNIQUE (Name)
) ENGINE=InnoDB;

-- Fixed, well-known GUIDs so the application can reference roles without a lookup.
-- See CounsellingApp.Domain.Constants.RoleIds in the C# code.
INSERT INTO Roles (Id, Name) VALUES
    ('11111111-1111-1111-1111-111111111111', 'Admin'),
    ('22222222-2222-2222-2222-222222222222', 'Student')
ON DUPLICATE KEY UPDATE Name = VALUES(Name);

-- ---------------------------------------------------------------------
-- Users (GUID only). No RoleId column here - see UserRole below.
-- PhoneNumber is unique (when provided) because users can log in with
-- either Email or PhoneNumber.
-- ---------------------------------------------------------------------
CREATE TABLE IF NOT EXISTS Users (
    Id                     CHAR(36)      NOT NULL PRIMARY KEY,
    FullName               VARCHAR(150)  NOT NULL,
    Email                  VARCHAR(150)  NOT NULL,
    PhoneNumber            VARCHAR(15)   NULL,
    PasswordHash           VARCHAR(255)  NOT NULL,
    NeetRank               INT           NULL,
    Category               VARCHAR(20)   NULL,
    IsEmailVerified        TINYINT(1)    NOT NULL DEFAULT 0,
    IsPhoneNumberVerified  TINYINT(1)    NOT NULL DEFAULT 0,
    IsActive               TINYINT(1)    NOT NULL DEFAULT 1,
    IsDeleted              TINYINT(1)    NOT NULL DEFAULT 0,
    CreatedOn              DATETIME      NOT NULL DEFAULT CURRENT_TIMESTAMP,
    UpdatedOn              DATETIME      NULL ON UPDATE CURRENT_TIMESTAMP,
    CONSTRAINT uq_users_email UNIQUE (Email),
    CONSTRAINT uq_users_phonenumber UNIQUE (PhoneNumber)
) ENGINE=InnoDB;

CREATE INDEX ix_users_email ON Users(Email);

-- ---------------------------------------------------------------------
-- UserRole - many-to-many junction between Users and Roles.
-- Composite primary key; hard-deleted (not soft-deleted) since removing a
-- role assignment just removes the mapping row.
-- ---------------------------------------------------------------------
CREATE TABLE IF NOT EXISTS UserRole (
    UserId     CHAR(36)  NOT NULL,
    RoleId     CHAR(36)  NOT NULL,
    CreatedOn  DATETIME  NOT NULL DEFAULT CURRENT_TIMESTAMP,
    PRIMARY KEY (UserId, RoleId),
    CONSTRAINT fk_userrole_user FOREIGN KEY (UserId) REFERENCES Users(Id) ON DELETE CASCADE,
    CONSTRAINT fk_userrole_role FOREIGN KEY (RoleId) REFERENCES Roles(Id) ON DELETE CASCADE
) ENGINE=InnoDB;

-- ---------------------------------------------------------------------
-- PasswordResetTokens - dual key. UserId is GUID (relates to Users).
-- `Token` column stores a SHA-256 hash, never the raw token.
-- ---------------------------------------------------------------------
CREATE TABLE IF NOT EXISTS PasswordResetTokens (
    Id          INT AUTO_INCREMENT PRIMARY KEY,
    Guid        CHAR(36)      NOT NULL UNIQUE DEFAULT (UUID()),
    UserId      CHAR(36)      NOT NULL,
    Token       VARCHAR(255)  NOT NULL,
    ExpiryDate  DATETIME      NOT NULL,
    IsUsed      TINYINT(1)    NOT NULL DEFAULT 0,
    IsDeleted   TINYINT(1)    NOT NULL DEFAULT 0,
    CreatedOn   DATETIME      NOT NULL DEFAULT CURRENT_TIMESTAMP,
    UpdatedOn   DATETIME      NULL ON UPDATE CURRENT_TIMESTAMP,
    CONSTRAINT fk_passwordresettokens_user FOREIGN KEY (UserId) REFERENCES Users(Id) ON DELETE CASCADE
) ENGINE=InnoDB;

CREATE INDEX ix_passwordresettokens_token ON PasswordResetTokens(Token);
CREATE INDEX ix_passwordresettokens_userid ON PasswordResetTokens(UserId);

-- ---------------------------------------------------------------------
-- RefreshTokens - dual key. UserId is GUID (relates to Users).
-- ---------------------------------------------------------------------
CREATE TABLE IF NOT EXISTS RefreshTokens (
    Id          INT AUTO_INCREMENT PRIMARY KEY,
    Guid        CHAR(36)      NOT NULL UNIQUE DEFAULT (UUID()),
    UserId      CHAR(36)      NOT NULL,
    Token       VARCHAR(500)  NOT NULL,
    ExpiryDate  DATETIME      NOT NULL,
    IsRevoked   TINYINT(1)    NOT NULL DEFAULT 0,
    IsDeleted   TINYINT(1)    NOT NULL DEFAULT 0,
    CreatedOn   DATETIME      NOT NULL DEFAULT CURRENT_TIMESTAMP,
    UpdatedOn   DATETIME      NULL ON UPDATE CURRENT_TIMESTAMP,
    CONSTRAINT fk_refreshtokens_user FOREIGN KEY (UserId) REFERENCES Users(Id) ON DELETE CASCADE
) ENGINE=InnoDB;

CREATE INDEX ix_refreshtokens_token ON RefreshTokens(Token(191));
CREATE INDEX ix_refreshtokens_userid ON RefreshTokens(UserId);

-- ---------------------------------------------------------------------
-- States - dual key
-- ---------------------------------------------------------------------
CREATE TABLE IF NOT EXISTS States (
    Id         INT AUTO_INCREMENT PRIMARY KEY,
    Guid       CHAR(36)      NOT NULL UNIQUE DEFAULT (UUID()),
    Name       VARCHAR(100)  NOT NULL,
    IsDeleted  TINYINT(1)    NOT NULL DEFAULT 0,
    CreatedOn  DATETIME      NOT NULL DEFAULT CURRENT_TIMESTAMP,
    UpdatedOn  DATETIME      NULL ON UPDATE CURRENT_TIMESTAMP,
    CONSTRAINT uq_states_name UNIQUE (Name)
) ENGINE=InnoDB;

-- ---------------------------------------------------------------------
-- Colleges - dual key. StateId is INT (relates to States, not Users/Roles).
-- ---------------------------------------------------------------------
CREATE TABLE IF NOT EXISTS Colleges (
    Id             INT AUTO_INCREMENT PRIMARY KEY,
    Guid           CHAR(36)      NOT NULL UNIQUE DEFAULT (UUID()),
    Name           VARCHAR(200)  NOT NULL,
    InstituteType  ENUM('Govt', 'Private', 'Deemed') NOT NULL,
    StateId        INT           NOT NULL,
    IsDeleted      TINYINT(1)    NOT NULL DEFAULT 0,
    CreatedOn      DATETIME      NOT NULL DEFAULT CURRENT_TIMESTAMP,
    UpdatedOn      DATETIME      NULL ON UPDATE CURRENT_TIMESTAMP,
    CONSTRAINT fk_colleges_state FOREIGN KEY (StateId) REFERENCES States(Id)
) ENGINE=InnoDB;

CREATE INDEX ix_colleges_stateid ON Colleges(StateId);
CREATE INDEX ix_colleges_institutetype ON Colleges(InstituteType);

-- ---------------------------------------------------------------------
-- Courses - dual key
-- ---------------------------------------------------------------------
CREATE TABLE IF NOT EXISTS Courses (
    Id         INT AUTO_INCREMENT PRIMARY KEY,
    Guid       CHAR(36)      NOT NULL UNIQUE DEFAULT (UUID()),
    Name       VARCHAR(150)  NOT NULL,
    Type       VARCHAR(20)   NOT NULL,   -- MD / MS / Diploma
    IsDeleted  TINYINT(1)    NOT NULL DEFAULT 0,
    CreatedOn  DATETIME      NOT NULL DEFAULT CURRENT_TIMESTAMP,
    UpdatedOn  DATETIME      NULL ON UPDATE CURRENT_TIMESTAMP
) ENGINE=InnoDB;

-- ---------------------------------------------------------------------
-- SeatAllotments - dual key. CollegeId / CourseId are INT
-- (they relate to Colleges / Courses, not Users/Roles).
-- ---------------------------------------------------------------------
CREATE TABLE IF NOT EXISTS SeatAllotments (
    Id                  INT AUTO_INCREMENT PRIMARY KEY,
    Guid                CHAR(36)      NOT NULL UNIQUE DEFAULT (UUID()),
    CollegeId           INT           NOT NULL,
    CourseId            INT           NOT NULL,
    Quota               VARCHAR(20)   NOT NULL DEFAULT 'AIQ',
    SeatCategory        VARCHAR(20)   NOT NULL,   -- Open, OBC, SC, ST, EWS, PwD
    TotalSeats          INT           NOT NULL DEFAULT 1,
    Round1ClosingRank   INT           NULL,
    Round2ClosingRank   INT           NULL,
    Round3ClosingRank   INT           NULL,
    AcademicYear        VARCHAR(9)    NOT NULL,   -- e.g. 2025-26
    IsDeleted           TINYINT(1)    NOT NULL DEFAULT 0,
    CreatedOn           DATETIME      NOT NULL DEFAULT CURRENT_TIMESTAMP,
    UpdatedOn           DATETIME      NULL ON UPDATE CURRENT_TIMESTAMP,
    CONSTRAINT fk_seatallotments_college FOREIGN KEY (CollegeId) REFERENCES Colleges(Id),
    CONSTRAINT fk_seatallotments_course FOREIGN KEY (CourseId) REFERENCES Courses(Id)
) ENGINE=InnoDB;

CREATE INDEX ix_seatallotments_category_rank ON SeatAllotments(SeatCategory, Round1ClosingRank);
CREATE INDEX ix_seatallotments_courseid ON SeatAllotments(CourseId);
