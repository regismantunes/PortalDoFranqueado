CREATE TABLE `portal_dev`.`produto` (
  `id` INT NOT NULL AUTO_INCREMENT,
  `idcolecao` INT NOT NULL,
  `idfamilia` INT NOT NULL,
  `foto` VARCHAR(255) NULL,
  `preco` DECIMAL(6,5) NULL,
  `idusuario` INT NOT NULL,
  PRIMARY KEY (`id`),
  INDEX `FK_produto_colecao_idx` (`idcolecao` ASC) VISIBLE,
  INDEX `FK_produto_familia_idx` (`idfamilia` ASC) VISIBLE,
  INDEX `FK_produto_usuario_idx` (`idusuario` ASC) VISIBLE,
  CONSTRAINT `FK_produto_colecao`
    FOREIGN KEY (`idcolecao`)
    REFERENCES `portal_dev`.`colecao` (`id`)
    ON DELETE CASCADE
    ON UPDATE NO ACTION,
  CONSTRAINT `FK_produto_familia`
    FOREIGN KEY (`idfamilia`)
    REFERENCES `portal_dev`.`familia` (`id`)
    ON DELETE NO ACTION
    ON UPDATE NO ACTION,
  CONSTRAINT `FK_produto_usuario`
    FOREIGN KEY (`idusuario`)
    REFERENCES `portal_dev`.`usuario` (`id`)
    ON DELETE NO ACTION
    ON UPDATE NO ACTION);
