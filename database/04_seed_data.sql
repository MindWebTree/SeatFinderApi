-- =====================================================================
-- Counselling App - Sample/demo data (SAFE TO SKIP IN PRODUCTION)
-- Loosely mirrors the sample "Seat Finder" data used for design review -
-- these are NOT real MCC allotment figures. Replace with real data before
-- using this for anything other than local testing.
--
-- States / Courses / Colleges / SeatAllotments are dual-key tables: we
-- don't specify Id or Guid here - the AUTO_INCREMENT int and the
-- DEFAULT (UUID()) guid both populate themselves automatically.
-- =====================================================================

USE counselling_app;

INSERT INTO States (Name) VALUES
    ('Delhi'), ('Karnataka'), ('Maharashtra'), ('Rajasthan'),
    ('Uttar Pradesh'), ('Telangana'), ('Gujarat'), ('Kerala')
ON DUPLICATE KEY UPDATE Name = VALUES(Name);

INSERT INTO Courses (Name, Type) VALUES
    ('MD Psychiatry', 'MD'),
    ('MS Orthopaedics', 'MS'),
    ('MS ENT', 'MS'),
    ('MD Emergency Medicine', 'MD'),
    ('MD Pathology', 'MD');

INSERT INTO Colleges (Name, InstituteType, StateId) VALUES
    ('MS Ramaiah Medical College',            'Private', (SELECT Id FROM States WHERE Name = 'Karnataka')),
    ('SMS Medical College',                   'Govt',    (SELECT Id FROM States WHERE Name = 'Rajasthan')),
    ('Sarojini Naidu Medical College',        'Govt',    (SELECT Id FROM States WHERE Name = 'Uttar Pradesh')),
    ('Osmania Medical College',               'Govt',    (SELECT Id FROM States WHERE Name = 'Telangana')),
    ('Government Medical College, Surat',     'Govt',    (SELECT Id FROM States WHERE Name = 'Gujarat')),
    ('PGIMER Dr RML Hospital',                'Govt',    (SELECT Id FROM States WHERE Name = 'Delhi')),
    ('UCMS & GTB Hospital',                   'Govt',    (SELECT Id FROM States WHERE Name = 'Delhi')),
    ('Government Medical College, Kozhikode', 'Govt',    (SELECT Id FROM States WHERE Name = 'Kerala'));

INSERT INTO SeatAllotments (CollegeId, CourseId, Quota, SeatCategory, TotalSeats, Round1ClosingRank, Round2ClosingRank, Round3ClosingRank, AcademicYear)
VALUES
    ((SELECT Id FROM Colleges WHERE Name = 'MS Ramaiah Medical College'),            (SELECT Id FROM Courses WHERE Name = 'MS Orthopaedics'),          'AIQ', 'Open', 5, 15137, 29192, 35930, '2025-26'),
    ((SELECT Id FROM Colleges WHERE Name = 'SMS Medical College'),                   (SELECT Id FROM Courses WHERE Name = 'MD Psychiatry'),            'AIQ', 'Open', 7, 15042, 30858, 40008, '2025-26'),
    ((SELECT Id FROM Colleges WHERE Name = 'Sarojini Naidu Medical College'),        (SELECT Id FROM Courses WHERE Name = 'MS ENT'),                   'AIQ', 'Open', 2, 14883, 29495, 33287, '2025-26'),
    ((SELECT Id FROM Colleges WHERE Name = 'Osmania Medical College'),               (SELECT Id FROM Courses WHERE Name = 'MS ENT'),                   'AIQ', 'Open', 4, 14842, 30807, 42333, '2025-26'),
    ((SELECT Id FROM Colleges WHERE Name = 'Government Medical College, Surat'),     (SELECT Id FROM Courses WHERE Name = 'MD Psychiatry'),            'AIQ', 'Open', 5, 15248, 26574, 35511, '2025-26'),
    ((SELECT Id FROM Colleges WHERE Name = 'PGIMER Dr RML Hospital'),                (SELECT Id FROM Courses WHERE Name = 'MD Pathology'),             'AIQ', 'Open', 2, 15364, 26061, 33189, '2025-26'),
    ((SELECT Id FROM Colleges WHERE Name = 'UCMS & GTB Hospital'),                   (SELECT Id FROM Courses WHERE Name = 'MD Psychiatry'),            'AIQ', 'Open', 3, 15427, 25775, 36129, '2025-26'),
    ((SELECT Id FROM Colleges WHERE Name = 'Government Medical College, Kozhikode'), (SELECT Id FROM Courses WHERE Name = 'MD Emergency Medicine'),    'AIQ', 'Open', 9, 15661, 31397, 45739, '2025-26');
