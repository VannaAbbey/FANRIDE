CREATE DATABASE  IF NOT EXISTS `fanride_db` /*!40100 DEFAULT CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci */ /*!80016 DEFAULT ENCRYPTION='N' */;
USE `fanride_db`;
-- MySQL dump 10.13  Distrib 8.0.40, for Win64 (x86_64)
--
-- Host: localhost    Database: fanride_db
-- ------------------------------------------------------
-- Server version	8.0.40

/*!40101 SET @OLD_CHARACTER_SET_CLIENT=@@CHARACTER_SET_CLIENT */;
/*!40101 SET @OLD_CHARACTER_SET_RESULTS=@@CHARACTER_SET_RESULTS */;
/*!40101 SET @OLD_COLLATION_CONNECTION=@@COLLATION_CONNECTION */;
/*!50503 SET NAMES utf8 */;
/*!40103 SET @OLD_TIME_ZONE=@@TIME_ZONE */;
/*!40103 SET TIME_ZONE='+00:00' */;
/*!40014 SET @OLD_UNIQUE_CHECKS=@@UNIQUE_CHECKS, UNIQUE_CHECKS=0 */;
/*!40014 SET @OLD_FOREIGN_KEY_CHECKS=@@FOREIGN_KEY_CHECKS, FOREIGN_KEY_CHECKS=0 */;
/*!40101 SET @OLD_SQL_MODE=@@SQL_MODE, SQL_MODE='NO_AUTO_VALUE_ON_ZERO' */;
/*!40111 SET @OLD_SQL_NOTES=@@SQL_NOTES, SQL_NOTES=0 */;

--
-- Table structure for table `bookings`
--

DROP TABLE IF EXISTS `bookings`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `bookings` (
  `Id` int NOT NULL AUTO_INCREMENT,
  `UserId` int DEFAULT NULL,
  `RideId` int DEFAULT NULL,
  `SeatTypeId` int DEFAULT NULL,
  `SeatCount` int DEFAULT NULL,
  `Price` decimal(10,2) DEFAULT NULL,
  `Status` varchar(50) DEFAULT NULL,
  `BookingDate` datetime DEFAULT CURRENT_TIMESTAMP,
  PRIMARY KEY (`Id`),
  KEY `UserId` (`UserId`),
  KEY `RideId` (`RideId`),
  KEY `SeatTypeId` (`SeatTypeId`),
  CONSTRAINT `bookings_ibfk_1` FOREIGN KEY (`UserId`) REFERENCES `users` (`Id`),
  CONSTRAINT `bookings_ibfk_2` FOREIGN KEY (`RideId`) REFERENCES `rides` (`Id`),
  CONSTRAINT `bookings_ibfk_3` FOREIGN KEY (`SeatTypeId`) REFERENCES `seattypes` (`Id`)
) ENGINE=InnoDB AUTO_INCREMENT=2 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `bookings`
--

LOCK TABLES `bookings` WRITE;
/*!40000 ALTER TABLE `bookings` DISABLE KEYS */;
INSERT INTO `bookings` VALUES (1,3,1,2,3,600.00,'Confirmed','2025-07-02 21:22:09');
/*!40000 ALTER TABLE `bookings` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `driverprofiles`
--

DROP TABLE IF EXISTS `driverprofiles`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `driverprofiles` (
  `Id` int NOT NULL AUTO_INCREMENT,
  `UserId` int DEFAULT NULL,
  `LicenseNumber` varchar(100) DEFAULT NULL,
  PRIMARY KEY (`Id`),
  KEY `UserId` (`UserId`),
  CONSTRAINT `driverprofiles_ibfk_1` FOREIGN KEY (`UserId`) REFERENCES `users` (`Id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `driverprofiles`
--

LOCK TABLES `driverprofiles` WRITE;
/*!40000 ALTER TABLE `driverprofiles` DISABLE KEYS */;
/*!40000 ALTER TABLE `driverprofiles` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `events`
--

DROP TABLE IF EXISTS `events`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `events` (
  `Id` int NOT NULL AUTO_INCREMENT,
  `Title` varchar(100) DEFAULT NULL,
  `Artist` varchar(100) DEFAULT NULL,
  `DateTime` datetime DEFAULT NULL,
  `Location` varchar(255) DEFAULT NULL,
  `Description` text,
  `ImageUrl` varchar(255) DEFAULT NULL,
  PRIMARY KEY (`Id`)
) ENGINE=InnoDB AUTO_INCREMENT=3 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `events`
--

LOCK TABLES `events` WRITE;
/*!40000 ALTER TABLE `events` DISABLE KEYS */;
INSERT INTO `events` VALUES (1,'Born Pink World Tour','Blackpink','2025-07-20 19:00:00','Philippine Arena','Blackpink live in concert!','/images/blackpink.jpg'),(2,'bts','bts','2025-07-02 21:22:00','Bulacan, Philippine Arena','bts','bts.jpg');
/*!40000 ALTER TABLE `events` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `interestchecks`
--

DROP TABLE IF EXISTS `interestchecks`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `interestchecks` (
  `Id` int NOT NULL AUTO_INCREMENT,
  `UserId` int DEFAULT NULL,
  `EventId` int DEFAULT NULL,
  `ExpressedAt` datetime DEFAULT CURRENT_TIMESTAMP,
  PRIMARY KEY (`Id`),
  KEY `UserId` (`UserId`),
  KEY `EventId` (`EventId`),
  CONSTRAINT `interestchecks_ibfk_1` FOREIGN KEY (`UserId`) REFERENCES `users` (`Id`),
  CONSTRAINT `interestchecks_ibfk_2` FOREIGN KEY (`EventId`) REFERENCES `events` (`Id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `interestchecks`
--

LOCK TABLES `interestchecks` WRITE;
/*!40000 ALTER TABLE `interestchecks` DISABLE KEYS */;
/*!40000 ALTER TABLE `interestchecks` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `landmarks`
--

DROP TABLE IF EXISTS `landmarks`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `landmarks` (
  `Id` int NOT NULL AUTO_INCREMENT,
  `Name` varchar(100) DEFAULT NULL,
  `Province` varchar(100) DEFAULT NULL,
  PRIMARY KEY (`Id`)
) ENGINE=InnoDB AUTO_INCREMENT=4 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `landmarks`
--

LOCK TABLES `landmarks` WRITE;
/*!40000 ALTER TABLE `landmarks` DISABLE KEYS */;
INSERT INTO `landmarks` VALUES (1,'SM North EDSA','Quezon City'),(2,'Alabang Town Center','Muntinlupa'),(3,'Ayala Malls Solenad','Sta. Rosa');
/*!40000 ALTER TABLE `landmarks` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `rides`
--

DROP TABLE IF EXISTS `rides`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `rides` (
  `Id` int NOT NULL AUTO_INCREMENT,
  `DriverId` int DEFAULT NULL,
  `EventId` int DEFAULT NULL,
  `LandmarkId` int DEFAULT NULL,
  `CarType` varchar(100) DEFAULT NULL,
  `PlateNumber` varchar(20) DEFAULT NULL,
  `CarSeatsTotal` int DEFAULT NULL,
  `DepartureTime` datetime DEFAULT NULL,
  `IsApproved` tinyint(1) DEFAULT '1',
  PRIMARY KEY (`Id`),
  KEY `DriverId` (`DriverId`),
  KEY `EventId` (`EventId`),
  KEY `LandmarkId` (`LandmarkId`),
  CONSTRAINT `rides_ibfk_1` FOREIGN KEY (`DriverId`) REFERENCES `users` (`Id`),
  CONSTRAINT `rides_ibfk_2` FOREIGN KEY (`EventId`) REFERENCES `events` (`Id`),
  CONSTRAINT `rides_ibfk_3` FOREIGN KEY (`LandmarkId`) REFERENCES `landmarks` (`Id`)
) ENGINE=InnoDB AUTO_INCREMENT=2 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `rides`
--

LOCK TABLES `rides` WRITE;
/*!40000 ALTER TABLE `rides` DISABLE KEYS */;
INSERT INTO `rides` VALUES (1,2,1,1,'White Honda','ABC-DSA',6,'0001-01-01 00:00:00',1);
/*!40000 ALTER TABLE `rides` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `rideseats`
--

DROP TABLE IF EXISTS `rideseats`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `rideseats` (
  `Id` int NOT NULL AUTO_INCREMENT,
  `RideId` int DEFAULT NULL,
  `SeatTypeId` int DEFAULT NULL,
  `TotalSeats` int DEFAULT NULL,
  PRIMARY KEY (`Id`),
  KEY `RideId` (`RideId`),
  KEY `SeatTypeId` (`SeatTypeId`),
  CONSTRAINT `rideseats_ibfk_1` FOREIGN KEY (`RideId`) REFERENCES `rides` (`Id`),
  CONSTRAINT `rideseats_ibfk_2` FOREIGN KEY (`SeatTypeId`) REFERENCES `seattypes` (`Id`)
) ENGINE=InnoDB AUTO_INCREMENT=3 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `rideseats`
--

LOCK TABLES `rideseats` WRITE;
/*!40000 ALTER TABLE `rideseats` DISABLE KEYS */;
INSERT INTO `rideseats` VALUES (1,1,1,1),(2,1,2,5);
/*!40000 ALTER TABLE `rideseats` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `seattypes`
--

DROP TABLE IF EXISTS `seattypes`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `seattypes` (
  `Id` int NOT NULL AUTO_INCREMENT,
  `TypeName` varchar(50) DEFAULT NULL,
  `TotalSeats` int DEFAULT NULL,
  `Price` decimal(10,2) DEFAULT '0.00',
  PRIMARY KEY (`Id`)
) ENGINE=InnoDB AUTO_INCREMENT=3 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `seattypes`
--

LOCK TABLES `seattypes` WRITE;
/*!40000 ALTER TABLE `seattypes` DISABLE KEYS */;
INSERT INTO `seattypes` VALUES (1,'Front Passenger',1,250.00),(2,'Rear Window',2,200.00);
/*!40000 ALTER TABLE `seattypes` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `users`
--

DROP TABLE IF EXISTS `users`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `users` (
  `Id` int NOT NULL AUTO_INCREMENT,
  `FirstName` varchar(50) DEFAULT NULL,
  `MiddleName` varchar(50) DEFAULT NULL,
  `LastName` varchar(50) DEFAULT NULL,
  `Email` varchar(100) DEFAULT NULL,
  `PasswordHash` text,
  `Province` varchar(100) DEFAULT NULL,
  `PhoneNumber` varchar(20) DEFAULT NULL,
  `Role` enum('Rider','Driver','Admin') NOT NULL,
  `CreatedAt` datetime DEFAULT CURRENT_TIMESTAMP,
  PRIMARY KEY (`Id`),
  UNIQUE KEY `Email` (`Email`)
) ENGINE=InnoDB AUTO_INCREMENT=4 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `users`
--

LOCK TABLES `users` WRITE;
/*!40000 ALTER TABLE `users` DISABLE KEYS */;
INSERT INTO `users` VALUES (1,'Admin','Admin','Admin','admin@gmail.com','$2a$11$3F4ZGCKi1EVxTk6PkmIoVuB7SPwa.FuVvX/kNZYKU3LshHZsUcxz2','Metro Manila','09282828281','Admin','2025-07-02 21:20:33'),(2,'Vanna Abbey','Fernando','Barrios','barriosvanna2@gmail.com','$2a$11$koIx6w45U9PQ3GN0G4go1.h.XJEkOiJfMUBHI2GBl7Ccnb2qMaF2a','Metro Manila','09660058605','Driver','2025-07-02 21:21:09'),(3,'Lhera','a','Lee','lheralee@gmail.com','$2a$11$8/MaGu2MPWVZALIk.2Jg7Oz5t6B1QluDP6ihyJmps3H8OH8b8hqBK','Cavite','09660058605','Rider','2025-07-02 21:21:33');
/*!40000 ALTER TABLE `users` ENABLE KEYS */;
UNLOCK TABLES;
/*!40103 SET TIME_ZONE=@OLD_TIME_ZONE */;

/*!40101 SET SQL_MODE=@OLD_SQL_MODE */;
/*!40014 SET FOREIGN_KEY_CHECKS=@OLD_FOREIGN_KEY_CHECKS */;
/*!40014 SET UNIQUE_CHECKS=@OLD_UNIQUE_CHECKS */;
/*!40101 SET CHARACTER_SET_CLIENT=@OLD_CHARACTER_SET_CLIENT */;
/*!40101 SET CHARACTER_SET_RESULTS=@OLD_CHARACTER_SET_RESULTS */;
/*!40101 SET COLLATION_CONNECTION=@OLD_COLLATION_CONNECTION */;
/*!40111 SET SQL_NOTES=@OLD_SQL_NOTES */;

-- Dump completed on 2025-07-02 21:30:55
