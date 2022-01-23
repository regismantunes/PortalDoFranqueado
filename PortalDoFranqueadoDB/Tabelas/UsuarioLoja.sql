CREATE TABLE `portal_dev`.`usuario_loja` (
  `idusuario` INT NOT NULL,
  `idloja` INT NOT NULL,
  PRIMARY KEY (`idusuario`, `idloja`),
  INDEX `loja_FOREIGNKEY_idx` (`idloja` ASC) VISIBLE,
  CONSTRAINT `usuario_FOREIGNKEY`
    FOREIGN KEY (`idusuario`)
    REFERENCES `portal_dev`.`usuario` (`id`)
    ON DELETE NO ACTION
    ON UPDATE NO ACTION,
  CONSTRAINT `loja_FOREIGNKEY`
    FOREIGN KEY (`idloja`)
    REFERENCES `portal_dev`.`loja` (`id`)
    ON DELETE NO ACTION
    ON UPDATE NO ACTION);
