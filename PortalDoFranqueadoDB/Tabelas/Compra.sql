CREATE TABLE `portal_dev`.`compra` (
  `id` INT NOT NULL AUTO_INCREMENT,
  `idloja` INT NOT NULL,
  `idcolecao` INT NOT NULL,
  `situacao` BIT NOT NULL,
  `idusuariorevisao` INT NOT NULL,
  PRIMARY KEY (`id`),
  INDEX `FK_compra_colecao_idx` (`idcolecao` ASC) VISIBLE,
  INDEX `FK_compra_loja_idx` (`idloja` ASC) VISIBLE,
  INDEX `FK_compra_usuariorevisao_idx` (`idusuariorevisao` ASC) VISIBLE,
  CONSTRAINT `FK_compra_colecao`
    FOREIGN KEY (`idcolecao`)
    REFERENCES `portal_dev`.`colecao` (`id`)
    ON DELETE CASCADE
    ON UPDATE NO ACTION,
  CONSTRAINT `FK_compra_loja`
    FOREIGN KEY (`idloja`)
    REFERENCES `portal_dev`.`loja` (`id`)
    ON DELETE NO ACTION
    ON UPDATE NO ACTION,
  CONSTRAINT `FK_compra_usuariorevisao`
    FOREIGN KEY (`idusuariorevisao`)
    REFERENCES `portal_dev`.`usuario` (`id`)
    ON DELETE NO ACTION
    ON UPDATE NO ACTION);
