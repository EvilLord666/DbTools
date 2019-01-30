-- Regions
INSERT INTO Region(Id, Name) VALUES(66, 'Свердловская область');
INSERT INTO Region(Id, Name) VALUES(74, 'Челябинская область');
INSERT INTO Region(Id, Name) VALUES(45, 'Курганская область');
INSERT INTO Region(Id, Name) VALUES(72, 'Тюменская область');

-- Cities
INSERT INTO City(Id, Name, RegionId) VALUES(343, 'Екатеринбург', 66);
INSERT INTO City(Id, Name, RegionId) VALUES(3439, 'Первоуральск', 66);
INSERT INTO City(Id, Name, RegionId) VALUES(3435, 'Нижний Тагил', 66);

INSERT INTO City(Id, Name, RegionId) VALUES(351, 'Челябинск', 74);
INSERT INTO City(Id, Name, RegionId) VALUES(3519, 'Магнитогорск', 74);
INSERT INTO City(Id, Name, RegionId) VALUES(35135, 'Миасс', 74);

INSERT INTO City(Id, Name, RegionId) VALUES(3452, 'Тюмень', 72);
INSERT INTO City(Id, Name, RegionId) VALUES(34511, 'Тобольск', 72);