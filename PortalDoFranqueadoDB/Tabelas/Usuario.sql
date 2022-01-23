CREATE TABLE `portal_dev`.`usuario` (
  `id` INT NOT NULL AUTO_INCREMENT,
  `nome` VARCHAR(50) NOT NULL,
  `situacao` BIT NOT NULL,
  `email` VARCHAR(100) NOT NULL,
  `senha` CHAR(52) NULL,
  `nivel` BIT NOT NULL,
  PRIMARY KEY (`id`),
  UNIQUE INDEX `email_UNIQUE` (`email` ASC) VISIBLE);
