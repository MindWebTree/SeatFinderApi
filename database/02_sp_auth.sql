-- =====================================================================
-- Counselling App - Auth stored procedures
-- Run after 01_schema.sql.
--
-- sp_RegisterUser inserts the user AND their default role assignment
-- (into UserRole) in the same call. sp_GetUserByEmail / sp_GetUserById
-- return a comma-separated Roles column via GROUP_CONCAT, since a user
-- can now hold more than one role.
-- =====================================================================

USE counselling_app;

DELIMITER //

DROP PROCEDURE IF EXISTS sp_RegisterUser //
CREATE PROCEDURE sp_RegisterUser (
    IN  p_full_name      VARCHAR(150),
    IN  p_email          VARCHAR(150),
    IN  p_phone_number   VARCHAR(15),
    IN  p_password_hash  VARCHAR(255),
    IN  p_role_id        CHAR(36),   -- default role to assign, e.g. RoleIds.Student
    IN  p_neet_rank      INT,
    IN  p_category       VARCHAR(20),
    OUT p_user_id        CHAR(36)
)
BEGIN
    SET p_user_id = UUID();

    INSERT INTO Users (Id, FullName, Email, PhoneNumber, PasswordHash, NeetRank, Category)
    VALUES (p_user_id, p_full_name, p_email, p_phone_number, p_password_hash, p_neet_rank, p_category);

    INSERT INTO UserRole (UserId, RoleId)
    VALUES (p_user_id, p_role_id);
END //

DROP PROCEDURE IF EXISTS sp_GetUserByEmail //
CREATE PROCEDURE sp_GetUserByEmail (
    IN p_email VARCHAR(150)
)
BEGIN
    SELECT
        u.Id, u.FullName, u.Email, u.PhoneNumber, u.PasswordHash,
        u.NeetRank, u.Category, u.IsEmailVerified, u.IsPhoneNumberVerified, u.IsActive,
        u.CreatedOn, u.UpdatedOn,
        GROUP_CONCAT(r.Name ORDER BY r.Name SEPARATOR ',') AS Roles
    FROM Users u
    LEFT JOIN UserRole ur ON ur.UserId = u.Id
    LEFT JOIN Roles r ON r.Id = ur.RoleId AND r.IsDeleted = 0
    WHERE u.Email = p_email AND u.IsDeleted = 0
    GROUP BY u.Id
    LIMIT 1;
END //

DROP PROCEDURE IF EXISTS sp_GetUserByPhoneNumber //
CREATE PROCEDURE sp_GetUserByPhoneNumber (
    IN p_phone_number VARCHAR(15)
)
BEGIN
    SELECT
        u.Id, u.FullName, u.Email, u.PhoneNumber, u.PasswordHash,
        u.NeetRank, u.Category, u.IsEmailVerified, u.IsPhoneNumberVerified, u.IsActive,
        u.CreatedOn, u.UpdatedOn,
        GROUP_CONCAT(r.Name ORDER BY r.Name SEPARATOR ',') AS Roles
    FROM Users u
    LEFT JOIN UserRole ur ON ur.UserId = u.Id
    LEFT JOIN Roles r ON r.Id = ur.RoleId AND r.IsDeleted = 0
    WHERE u.PhoneNumber = p_phone_number AND u.IsDeleted = 0
    GROUP BY u.Id
    LIMIT 1;
END //

DROP PROCEDURE IF EXISTS sp_GetUserById //
CREATE PROCEDURE sp_GetUserById (
    IN p_user_id CHAR(36)
)
BEGIN
    SELECT
        u.Id, u.FullName, u.Email, u.PhoneNumber, u.PasswordHash,
        u.NeetRank, u.Category, u.IsEmailVerified, u.IsPhoneNumberVerified, u.IsActive,
        u.CreatedOn, u.UpdatedOn,
        GROUP_CONCAT(r.Name ORDER BY r.Name SEPARATOR ',') AS Roles
    FROM Users u
    LEFT JOIN UserRole ur ON ur.UserId = u.Id
    LEFT JOIN Roles r ON r.Id = ur.RoleId AND r.IsDeleted = 0
    WHERE u.Id = p_user_id AND u.IsDeleted = 0
    GROUP BY u.Id
    LIMIT 1;
END //

DROP PROCEDURE IF EXISTS sp_UpdateUserPassword //
CREATE PROCEDURE sp_UpdateUserPassword (
    IN p_user_id       CHAR(36),
    IN p_password_hash VARCHAR(255)
)
BEGIN
    UPDATE Users
    SET PasswordHash = p_password_hash
    WHERE Id = p_user_id;
END //

DROP PROCEDURE IF EXISTS sp_CreatePasswordResetToken //
CREATE PROCEDURE sp_CreatePasswordResetToken (
    IN  p_user_id     CHAR(36),
    IN  p_token       VARCHAR(255),
    IN  p_expiry_date DATETIME,
    OUT p_token_id    INT
)
BEGIN
    -- Invalidate any previous unused reset tokens for this user first.
    UPDATE PasswordResetTokens
    SET IsUsed = 1
    WHERE UserId = p_user_id AND IsUsed = 0;

    -- Guid is filled in automatically via the column's DEFAULT (UUID()).
    INSERT INTO PasswordResetTokens (UserId, Token, ExpiryDate)
    VALUES (p_user_id, p_token, p_expiry_date);

    SET p_token_id = LAST_INSERT_ID();
END //

DROP PROCEDURE IF EXISTS sp_GetPasswordResetToken //
CREATE PROCEDURE sp_GetPasswordResetToken (
    IN p_token VARCHAR(255)
)
BEGIN
    SELECT Id, Guid, UserId, Token, ExpiryDate, IsUsed, CreatedOn, UpdatedOn
    FROM PasswordResetTokens
    WHERE Token = p_token AND IsDeleted = 0
    LIMIT 1;
END //

DROP PROCEDURE IF EXISTS sp_MarkPasswordResetTokenUsed //
CREATE PROCEDURE sp_MarkPasswordResetTokenUsed (
    IN p_token_id INT
)
BEGIN
    UPDATE PasswordResetTokens
    SET IsUsed = 1
    WHERE Id = p_token_id;
END //

DROP PROCEDURE IF EXISTS sp_CreateRefreshToken //
CREATE PROCEDURE sp_CreateRefreshToken (
    IN  p_user_id          CHAR(36),
    IN  p_token             VARCHAR(500),
    IN  p_expiry_date       DATETIME,
    OUT p_refresh_token_id  INT
)
BEGIN
    -- Guid is filled in automatically via the column's DEFAULT (UUID()).
    INSERT INTO RefreshTokens (UserId, Token, ExpiryDate)
    VALUES (p_user_id, p_token, p_expiry_date);

    SET p_refresh_token_id = LAST_INSERT_ID();
END //

DROP PROCEDURE IF EXISTS sp_GetRefreshToken //
CREATE PROCEDURE sp_GetRefreshToken (
    IN p_token VARCHAR(500)
)
BEGIN
    SELECT Id, Guid, UserId, Token, ExpiryDate, IsRevoked, CreatedOn, UpdatedOn
    FROM RefreshTokens
    WHERE Token = p_token AND IsDeleted = 0
    LIMIT 1;
END //

DROP PROCEDURE IF EXISTS sp_RevokeRefreshToken //
CREATE PROCEDURE sp_RevokeRefreshToken (
    IN p_token VARCHAR(500)
)
BEGIN
    UPDATE RefreshTokens
    SET IsRevoked = 1
    WHERE Token = p_token;
END //

DELIMITER ;
