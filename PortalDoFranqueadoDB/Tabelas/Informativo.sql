CREATE TABLE `portal_dev`.`informativo` (
  `id` INT NOT NULL AUTO_INCREMENT,
  `titulo` VARCHAR(100) NULL,
  `datahora` DATETIME NOT NULL,
  `texto` MEDIUMTEXT NULL,
  `tipo` BIT NOT NULL,
  `idusuario` INT NOT NULL,
  PRIMARY KEY (`id`),
  INDEX `usuario_FOREIGNKEY_idx` (`idusuario` ASC) VISIBLE,
  CONSTRAINT `FK_informativo_usuario`
    FOREIGN KEY (`idusuario`)
    REFERENCES `portal_dev`.`usuario` (`id`)
    ON DELETE NO ACTION
    ON UPDATE NO ACTION);
