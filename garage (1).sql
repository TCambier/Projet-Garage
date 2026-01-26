-- phpMyAdmin SQL Dump
-- version 5.2.1
-- https://www.phpmyadmin.net/
--
-- Hôte : 127.0.0.1:3306
-- Généré le : ven. 12 déc. 2025 à 15:58
-- Version du serveur : 8.3.0
-- Version de PHP : 8.2.18

SET SQL_MODE = "NO_AUTO_VALUE_ON_ZERO";
START TRANSACTION;
SET time_zone = "+00:00";


/*!40101 SET @OLD_CHARACTER_SET_CLIENT=@@CHARACTER_SET_CLIENT */;
/*!40101 SET @OLD_CHARACTER_SET_RESULTS=@@CHARACTER_SET_RESULTS */;
/*!40101 SET @OLD_COLLATION_CONNECTION=@@COLLATION_CONNECTION */;
/*!40101 SET NAMES utf8mb4 */;

--
-- Base de données : `garage`
--

DELIMITER $$
--
-- Procédures
--
DROP PROCEDURE IF EXISTS `ajouter_employe`$$
CREATE DEFINER=`root`@`localhost` PROCEDURE `ajouter_employe` (IN `p_statut` VARCHAR(20), IN `p_nom` VARCHAR(50), IN `p_prenom` VARCHAR(50))   BEGIN
  DECLARE v_user VARCHAR(64);
  DECLARE v_base Varchar(64);
  DECLARE v_mdp VARCHAR(64);
  

  SET v_user = CONCAT(LEFT(p_prenom, 1), '.', p_nom);
  SET v_base = "Nouveau_Mdp";
  SET v_mdp = SUBSTRING(SHA2(v_base, 512), 1, 12);

  INSERT INTO utilisateur(statut, nom, prenom, Mdp, Identifiant)
  VALUES (p_statut, p_nom, p_prenom, v_mdp, v_user);
END$$

DROP PROCEDURE IF EXISTS `utiliser_piece_pour_entretien`$$
CREATE DEFINER=`root`@`localhost` PROCEDURE `utiliser_piece_pour_entretien` (IN `p_id_entretien` INT, IN `p_id_piece` INT, IN `p_quantite` INT)   BEGIN
  DECLARE v_dispo INT;
  DECLARE v_prix DECIMAL(15,2);

  IF p_quantite IS NULL OR p_quantite <= 0 THEN
    SIGNAL SQLSTATE '45000' SET MESSAGE_TEXT = 'Quantite invalide (doit etre > 0)';
  END IF;

  START TRANSACTION;

  -- Lock la ligne pièce (évite les conflits en concurrence)
  SELECT Nb_Piece, Prix_Unite
    INTO v_dispo, v_prix
  FROM piece
  WHERE Id = p_id_piece
  FOR UPDATE;

  IF v_dispo IS NULL THEN
    ROLLBACK;
    SIGNAL SQLSTATE '45000' SET MESSAGE_TEXT = 'Piece inexistante';
  END IF;

  IF v_dispo < p_quantite THEN
    ROLLBACK;
    SIGNAL SQLSTATE '45000' SET MESSAGE_TEXT = 'Stock insuffisant';
  END IF;

  -- Décrément stock
  UPDATE piece
  SET Nb_Piece = Nb_Piece - p_quantite
  WHERE Id = p_id_piece;

  -- Insère ou ajoute à la quantité (PK (Id_Entretien, Id_Piece) => 1 ligne par pièce/entretien)
  INSERT INTO utilise (Id_Entretien, Id_Piece, Prix_Unite_Applique, Quantite)
  VALUES (p_id_entretien, p_id_piece, v_prix, p_quantite)
  ON DUPLICATE KEY UPDATE
    Quantite = Quantite + VALUES(Quantite);

  COMMIT;
END$$

DELIMITER ;

-- --------------------------------------------------------

--
-- Structure de la table `entretien`
--

DROP TABLE IF EXISTS `entretien`;
CREATE TABLE IF NOT EXISTS `entretien` (
  `Id` int NOT NULL AUTO_INCREMENT,
  `Type` varchar(50) NOT NULL,
  `Date_Intervention` date NOT NULL,
  `Cout` decimal(15,2) NOT NULL,
  `Prix` decimal(15,2) NOT NULL,
  `Kilometrage` int NOT NULL,
  `Statut` varchar(50) NOT NULL,
  `Immatriculation` varchar(9) NOT NULL,
  `Id_Utilisateur` int NOT NULL,
  PRIMARY KEY (`Id`),
  KEY `Immatriculation` (`Immatriculation`),
  KEY `Id_Utilisateur` (`Id_Utilisateur`)
) ENGINE=InnoDB AUTO_INCREMENT=8 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;

--
-- Déchargement des données de la table `entretien`
--

INSERT INTO `entretien` (`Id`, `Type`, `Date_Intervention`, `Cout`, `Prix`, `Kilometrage`, `Statut`, `Immatriculation`, `Id_Utilisateur`) VALUES
(3, 'Vidange', '2025-12-11', 20.00, 200.00, 71000, 'Terminé', 'XX-AAA-XX', 1),
(4, 'plaquette de frein', '2025-12-11', 60.00, 300.00, 80000, 'Terminé', 'XX-AAA-XX', 1),
(5, 'Changement Moteur', '2025-12-12', 23000.00, 959546.00, 456529052, 'Terminé', 'ZE-324-KG', 1),
(6, 'Ampoule phare', '2025-12-12', 5.00, 30.00, 10000, 'Terminé', 'KH-235-PM', 2);

-- --------------------------------------------------------

--
-- Structure de la table `piece`
--

DROP TABLE IF EXISTS `piece`;
CREATE TABLE IF NOT EXISTS `piece` (
  `Id` int NOT NULL AUTO_INCREMENT,
  `Libelle` varchar(50) DEFAULT NULL,
  `Prix_Unite` decimal(15,2) DEFAULT NULL,
  `Nb_Piece` int NOT NULL DEFAULT '0',
  PRIMARY KEY (`Id`),
  UNIQUE KEY `Libelle` (`Libelle`)
) ;

--
-- Déchargement des données de la table `piece`
--

INSERT INTO `piece` (`Id`, `Libelle`, `Prix_Unite`, `Nb_Piece`) VALUES
(1, 'Moteur M4', 45200.00, 3),
(2, 'Ampoule phare', 5.00, 45);

-- --------------------------------------------------------

--
-- Structure de la table `utilisateur`
--

DROP TABLE IF EXISTS `utilisateur`;
CREATE TABLE IF NOT EXISTS `utilisateur` (
  `Id` int NOT NULL AUTO_INCREMENT,
  `Statut` varchar(50) NOT NULL,
  `Nom` varchar(50) NOT NULL,
  `Prenom` varchar(50) NOT NULL,
  `Mdp` varchar(256) NOT NULL,
  `Identifiant` varchar(50) NOT NULL,
  PRIMARY KEY (`Id`),
  UNIQUE KEY `Identifiant` (`Identifiant`)
) ENGINE=InnoDB AUTO_INCREMENT=3 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;

--
-- Déchargement des données de la table `utilisateur`
--

INSERT INTO `utilisateur` (`Id`, `Statut`, `Nom`, `Prenom`, `Mdp`, `Identifiant`) VALUES
(1, 'Patron', 'Durand', 'Jean', '240be518fabd2724ddb6f04eeb1da5967448d7e831c08c8fa822809f74c720a9', 'J.Durand'),
(2, 'Client', 'Crosnier', 'Yoann', 'da30e45705f79aa7555b538e098d0ab284fbc6c890f92ed8ad1722b63aa69201', 'yolo_zyrt');

-- --------------------------------------------------------

--
-- Structure de la table `utilise`
--

DROP TABLE IF EXISTS `utilise`;
CREATE TABLE IF NOT EXISTS `utilise` (
  `Id_Entretien` int NOT NULL,
  `Id_Piece` int NOT NULL,
  `Prix_Unite_Applique` decimal(15,2) DEFAULT NULL,
  `Quantite` int NOT NULL,
  PRIMARY KEY (`Id_Entretien`,`Id_Piece`),
  KEY `Id_Piece` (`Id_Piece`)
) ;

-- --------------------------------------------------------

--
-- Structure de la table `voiture`
--

DROP TABLE IF EXISTS `voiture`;
CREATE TABLE IF NOT EXISTS `voiture` (
  `Immatriculation` varchar(9) NOT NULL,
  `Marque` varchar(20) NOT NULL,
  `Model` varchar(50) NOT NULL,
  `Annee` int NOT NULL,
  `Kilometrage` int NOT NULL,
  `Statut` varchar(50) NOT NULL,
  `Type_Carburant` varchar(50) NOT NULL,
  `Id_Utilisateur` int NOT NULL,
  PRIMARY KEY (`Immatriculation`),
  KEY `Id_Utilisateur` (`Id_Utilisateur`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;

--
-- Déchargement des données de la table `voiture`
--

INSERT INTO `voiture` (`Immatriculation`, `Marque`, `Model`, `Annee`, `Kilometrage`, `Statut`, `Type_Carburant`, `Id_Utilisateur`) VALUES
('gf123qs', 'bmw', 'X5', 2025, 30000, 'EnService', 'Essence', 2),
('GH-234-GD', 'BMW ', 'X5', 2025, 45224, 'EnService', 'Hybride', 2),
('KH-235-PM', 'BMW', 'M5', 2025, 4562, 'EnService', 'Essence', 2),
('XX-AAA-XX', 'Renault', 'Clio', 1999, 70000, 'EnService', 'Diesel', 1),
('ZE-324-KG', 'BMW', 'M4', 2025, 4121854, 'EnService', 'Diesel', 1);

--
-- Contraintes pour les tables déchargées
--

--
-- Contraintes pour la table `entretien`
--
ALTER TABLE `entretien`
  ADD CONSTRAINT `entretien_ibfk_1` FOREIGN KEY (`Immatriculation`) REFERENCES `voiture` (`Immatriculation`) ON DELETE CASCADE ON UPDATE CASCADE,
  ADD CONSTRAINT `entretien_ibfk_2` FOREIGN KEY (`Id_Utilisateur`) REFERENCES `utilisateur` (`Id`) ON DELETE CASCADE ON UPDATE CASCADE;

--
-- Contraintes pour la table `utilise`
--
ALTER TABLE `utilise`
  ADD CONSTRAINT `utilise_ibfk_1` FOREIGN KEY (`Id_Piece`) REFERENCES `piece` (`Id`) ON DELETE CASCADE ON UPDATE CASCADE,
  ADD CONSTRAINT `utilise_ibfk_2` FOREIGN KEY (`Id_Entretien`) REFERENCES `entretien` (`Id`) ON DELETE CASCADE ON UPDATE CASCADE;

--
-- Contraintes pour la table `voiture`
--
ALTER TABLE `voiture`
  ADD CONSTRAINT `voiture_ibfk_1` FOREIGN KEY (`Id_Utilisateur`) REFERENCES `utilisateur` (`Id`) ON DELETE CASCADE ON UPDATE CASCADE;
COMMIT;

/*!40101 SET CHARACTER_SET_CLIENT=@OLD_CHARACTER_SET_CLIENT */;
/*!40101 SET CHARACTER_SET_RESULTS=@OLD_CHARACTER_SET_RESULTS */;
/*!40101 SET COLLATION_CONNECTION=@OLD_COLLATION_CONNECTION */;
