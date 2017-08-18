CREATE TABLE `files` (
  `id` int(11) NOT NULL AUTO_INCREMENT,
  `Title` NVARCHAR(1000) DEFAULT NULL,
  `path` NVARCHAR(400) NOT NULL,
  `Directory` NVARCHAR(400) NOT NULL,
  `MD5` varchar(32) NOT NULL,
  `Version` varchar(15) DEFAULT NULL,
  `Content` mediumtext,
  `Innercontent` mediumtext,
  `Date` varchar(100) DEFAULT NULL,
  PRIMARY KEY (`id`,`MD5`,`path`,`Directory`),
  UNIQUE KEY `id_UNIQUE` (`id`),
  UNIQUE KEY `path_UNIQUE` (`path`),
  UNIQUE KEY `MD5_UNIQUE` (`MD5`),
  FULLTEXT KEY `Content` (`Content`,`Innercontent`),
  FULLTEXT KEY `Content_2` (`Content`),
  FULLTEXT KEY `Innercontent` (`Innercontent`)
) ENGINE=InnoDB AUTO_INCREMENT=934 DEFAULT CHARSET=utf8;
