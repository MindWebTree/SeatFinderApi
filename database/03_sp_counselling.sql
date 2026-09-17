-- =====================================================================
-- Counselling App - Counselling logic stored procedures
-- Run after 01_schema.sql.
-- CourseId / StateId are INT here - they relate to Courses / States,
-- which are dual-key tables, not Users/Roles.
-- =====================================================================

USE counselling_app;

DELIMITER //

DROP PROCEDURE IF EXISTS sp_SearchSeats //
CREATE PROCEDURE sp_SearchSeats (
    IN p_rank            INT,
    IN p_category         VARCHAR(20),
    IN p_institute_types  VARCHAR(100),  -- comma separated e.g. 'Govt,Private,Deemed'; NULL/'' = all
    IN p_course_id        INT,           -- NULL = all
    IN p_state_id         INT            -- NULL = all
)
BEGIN
    SELECT
        sa.Id                   AS SeatId,
        sa.Guid                 AS SeatGuid,
        c.Name                  AS CollegeName,
        c.InstituteType,
        s.Name                  AS StateName,
        co.Name                 AS CourseName,
        sa.SeatCategory,
        sa.TotalSeats,
        sa.Round1ClosingRank,
        sa.Round2ClosingRank,
        sa.Round3ClosingRank
    FROM SeatAllotments sa
    JOIN Colleges c  ON c.Id = sa.CollegeId  AND c.IsDeleted = 0
    JOIN States   s  ON s.Id = c.StateId     AND s.IsDeleted = 0
    JOIN Courses  co ON co.Id = sa.CourseId  AND co.IsDeleted = 0
    WHERE
        sa.IsDeleted = 0
        -- Category eligibility: General/Open candidates can only take Open seats;
        -- reserved-category candidates can take Open seats plus their own category.
        AND (
            sa.SeatCategory = 'Open'
            OR (
                UPPER(p_category) NOT IN ('OC', 'GENERAL', 'OPEN')
                AND sa.SeatCategory = p_category
            )
        )
        AND (p_institute_types IS NULL OR p_institute_types = '' OR FIND_IN_SET(c.InstituteType, p_institute_types) > 0)
        AND (p_course_id IS NULL OR sa.CourseId = p_course_id)
        AND (p_state_id IS NULL OR c.StateId = p_state_id)
    ORDER BY sa.Round1ClosingRank IS NULL, sa.Round1ClosingRank ASC;
END //

DROP PROCEDURE IF EXISTS sp_GetStates //
CREATE PROCEDURE sp_GetStates ()
BEGIN
    SELECT
        Id   AS StateId,
        Guid AS StateGuid,
        Name AS StateName
    FROM States
    WHERE IsDeleted = 0
    ORDER BY Name;
END //

DROP PROCEDURE IF EXISTS sp_GetCourses //
CREATE PROCEDURE sp_GetCourses ()
BEGIN
    SELECT
        Id   AS CourseId,
        Guid AS CourseGuid,
        Name AS CourseName,
        Type AS CourseType
    FROM Courses
    WHERE IsDeleted = 0
    ORDER BY Name;
END //

DELIMITER ;
