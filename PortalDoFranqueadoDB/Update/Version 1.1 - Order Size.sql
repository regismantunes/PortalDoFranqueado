/*BEGIN
	ALTER TABLE Family_Size
		ADD [Order] SMALLINT

	DECLARE @Order SMALLINT
		,	@FamilyId INT
		,	@FamilyIdOld INT
		,	@SizeId VARCHAR(5)

	DECLARE cFamilySize CURSOR FOR SELECT FamilyId, SizeId
									FROM Family_Size
									ORDER BY FamilyId, SizeId
									FOR UPDATE OF [Order]

	OPEN cFamilySize

	SET @FamilyIdOld = 0
	FETCH NEXT FROM cFamilySize INTO @FamilyId, @SizeId
	WHILE @@FETCH_STATUS = 0
	BEGIN
		
		IF @FamilyIdOld <> @FamilyId
		BEGIN
			SET @FamilyIdOld = @FamilyId
			SET @Order = 0
		END

		SET @Order = CASE WHEN @FamilyId = 9 THEN
							CASE @SizeId
								WHEN '38'		THEN 0
								WHEN '39'		THEN 1
								WHEN '40'		THEN 2
								WHEN '41'		THEN 3
								WHEN '42'		THEN 4
								WHEN '43'		THEN 5
							END
						ELSE
							CASE @SizeId
								WHEN 'Único'	THEN 0
								WHEN 'P'		THEN 0
								WHEN 'M'		THEN 1
								WHEN 'G'		THEN 2
								WHEN 'GG'		THEN 3
								WHEN 'G1'		THEN 4
								WHEN 'G2'		THEN 5
								WHEN 'G3'		THEN 6
								WHEN '36'		THEN 0
								WHEN '38'		THEN 1
								WHEN '40'		THEN 2
								WHEN '42'		THEN 3
								WHEN '44'		THEN 4
								WHEN '46'		THEN 5
								WHEN '48'		THEN 6
								WHEN '37/38'	THEN 0
								WHEN '39/40'	THEN 1
								WHEN '41/42'	THEN 2
								WHEN '43/44'	THEN 3
							END
						END

		UPDATE Family_Size
			SET [Order] = @Order
			WHERE CURRENT OF cFamilySize

		FETCH NEXT FROM cFamilySize INTO @FamilyId, @SizeId
	END

	CLOSE cFamilySize
	DEALLOCATE cFamilySize

	ALTER TABLE Family_Size
		ALTER COLUMN [Order] SMALLINT NOT NULL
END*/