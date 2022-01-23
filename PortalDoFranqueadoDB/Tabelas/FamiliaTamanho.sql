CREATE TABLE `portal_dev`.`familia_tamanho` (
  `idfamilia` INT NOT NULL,
  `idtamanho` VARCHAR(3) NOT NULL,
  PRIMARY KEY (`idfamilia`, `idtamanho`)
  CONSTRAINT `familia_FOREIGNKEY` FOREIGN KEY (`idfamilia`) REFERENCES `portal_dev`.`familia` (`id`) ON DELETE CASCADE);
  