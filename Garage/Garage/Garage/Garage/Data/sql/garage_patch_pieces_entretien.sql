-- Patch SQL (MySQL 8.x)
-- Objectif:
-- 1) N'afficher/sélectionner côté appli que les pièces disponibles (Nb_Piece > 0)
-- 2) Garantir en base que entretien.Cout >= somme(prix_unite_applique * quantite)

-- Vue utilitaire (optionnelle)
DROP VIEW IF EXISTS pieces_disponibles;
CREATE VIEW pieces_disponibles AS
SELECT Id, Libelle, Prix_Unite, Nb_Piece
FROM piece
WHERE Nb_Piece > 0;

DELIMITER $$

DROP TRIGGER IF EXISTS utilise_ai_cout_min $$
CREATE TRIGGER utilise_ai_cout_min
AFTER INSERT ON utilise
FOR EACH ROW
BEGIN
  DECLARE v_total DECIMAL(15,2);

  SELECT IFNULL(SUM(Prix_Unite_Applique * Quantite), 0)
    INTO v_total
  FROM utilise
  WHERE Id_Entretien = NEW.Id_Entretien;

  UPDATE entretien
  SET Cout = GREATEST(Cout, v_total)
  WHERE Id = NEW.Id_Entretien;
END $$

DROP TRIGGER IF EXISTS utilise_au_cout_min $$
CREATE TRIGGER utilise_au_cout_min
AFTER UPDATE ON utilise
FOR EACH ROW
BEGIN
  DECLARE v_total DECIMAL(15,2);

  SELECT IFNULL(SUM(Prix_Unite_Applique * Quantite), 0)
    INTO v_total
  FROM utilise
  WHERE Id_Entretien = NEW.Id_Entretien;

  UPDATE entretien
  SET Cout = GREATEST(Cout, v_total)
  WHERE Id = NEW.Id_Entretien;
END $$

DROP TRIGGER IF EXISTS utilise_ad_cout_min $$
CREATE TRIGGER utilise_ad_cout_min
AFTER DELETE ON utilise
FOR EACH ROW
BEGIN
  DECLARE v_total DECIMAL(15,2);

  SELECT IFNULL(SUM(Prix_Unite_Applique * Quantite), 0)
    INTO v_total
  FROM utilise
  WHERE Id_Entretien = OLD.Id_Entretien;

  UPDATE entretien
  -- si des pièces ont été retirées, on n'abaisse pas le coût: on force juste un minimum
  SET Cout = GREATEST(Cout, v_total)
  WHERE Id = OLD.Id_Entretien;
END $$

DELIMITER ;
