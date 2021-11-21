SET GLOBAL max_allowed_packet=64000000;
CREATE TABLE `stock` (
  `Id` int(11) NOT NULL AUTO_INCREMENT,
  `Symbol` varchar(255) CHARACTER SET utf8mb4 NOT NULL,
  `Description` longtext CHARACTER SET utf8mb4 NOT NULL,
  `Exchange` longtext CHARACTER SET utf8mb4 NOT NULL,
  `FirstUpdate` datetime(6) DEFAULT NULL,
  `LastUpdate` datetime(6) DEFAULT NULL,
  `Status` int(11) DEFAULT NULL,
  `PreviousDayClose` double DEFAULT NULL,
  `PreviousDay` datetime(6) DEFAULT NULL,
  `SplitFactor` double DEFAULT NULL,
  PRIMARY KEY (`Id`),
  UNIQUE KEY `IX_Stock_Symbol` (`Symbol`)
) ENGINE=InnoDB AUTO_INCREMENT=10222 DEFAULT CHARSET=latin1;

CREATE TABLE `stockquote` (
  `Id` int(11) NOT NULL AUTO_INCREMENT,
  `StockId` int(11) NOT NULL,
  `Timestamp` datetime(6) NOT NULL,
  `Bid` double NOT NULL,
  `BidSize` int(11) NOT NULL,
  `Ask` double NOT NULL,
  `AskSize` int(11) NOT NULL,
  `Volume` bigint(20) NOT NULL,
  `SplitFactor` double NOT NULL,
  PRIMARY KEY (`Id`),
  UNIQUE KEY `IX_StockQuote_Timestamp_StockId` (`Timestamp`,`StockId`),
  KEY `IX_StockQuote_StockId` (`StockId`),
  KEY `IX_StockQuote_Timestamp` (`Timestamp`),
  CONSTRAINT `FK_StockQuote_Stock_StockId` FOREIGN KEY (`StockId`) REFERENCES `stock` (`Id`) ON DELETE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=latin1;