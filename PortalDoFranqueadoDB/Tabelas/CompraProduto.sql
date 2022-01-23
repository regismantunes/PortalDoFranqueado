CREATE TABLE `portal_dev`.`compra_produto` (
  `idcompra` INT NOT NULL,
  `item` INT NOT NULL,
  `idproduto` INT NOT NULL,
  `idtamanho` VARCHAR(3) NOT NULL,
  `quantidade` INT NOT NULL,
  PRIMARY KEY (`idcompra`, `item`),
  INDEX `FK_compra_produto_produto_idx` (`idproduto` ASC) VISIBLE,
  CONSTRAINT `FK_compra_produto_compra`
    FOREIGN KEY (`idcompra`)
    REFERENCES `portal_dev`.`compra` (`id`)
    ON DELETE CASCADE
    ON UPDATE NO ACTION,
  CONSTRAINT `FK_compra_produto_produto`
    FOREIGN KEY (`idproduto`)
    REFERENCES `portal_dev`.`produto` (`id`)
    ON DELETE NO ACTION
    ON UPDATE NO ACTION);
