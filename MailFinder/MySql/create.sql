CREATE TABLE `files` (
  `id` int(11) NOT NULL AUTO_INCREMENT,
  `path` varchar(500) NOT NULL,
  `MD5` varchar(32) NOT NULL,
  `Version` varchar(15) DEFAULT NULL,
  `Content` mediumtext,
  `Innercontent` mediumtext,
  PRIMARY KEY (`id`,`path`,`MD5`),
  UNIQUE KEY `id_UNIQUE` (`id`),
  UNIQUE KEY `path_UNIQUE` (`path`),
  UNIQUE KEY `MD5_UNIQUE` (`MD5`),
  FULLTEXT KEY `Content` (`Content`,`Innercontent`)
) ENGINE=InnoDB AUTO_INCREMENT=5 DEFAULT CHARSET=utf8;
