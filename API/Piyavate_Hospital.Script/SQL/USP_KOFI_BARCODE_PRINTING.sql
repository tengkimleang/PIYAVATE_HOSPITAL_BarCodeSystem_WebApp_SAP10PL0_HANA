
Alter PROC USP_KOFI_BARCODE_PRINTING
	@docEntry As INT
AS
BEGIN
	SELECT ROW_NUMBER() OVER(ORDER BY DocNum) As #No,DocNum,A.DocDate,CardCode,CardName,A.NumAtCard
		,B.ItemCode,B.Dscription,B.Quantity,B.Price,B.LineTotal,B.UomCode,A.DocTotal,A.Comments
	FROM OPDN A
		LEFT JOIN PDN1 B ON A.DocEntry=B.DocEntry
	WHERE A.DocEntry=@docEntry 
END