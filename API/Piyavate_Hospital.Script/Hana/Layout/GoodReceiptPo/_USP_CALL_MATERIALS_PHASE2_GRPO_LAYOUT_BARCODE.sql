ALTER PROCEDURE "TRIWALL_TRAINKEY"._USP_CALL_MATERIALS_PHASE2_GRPO_LAYOUT_BARCODE(
	 in DTYPE NVARCHAR(250)
	,in par1 NVARCHAR(250)
	,in par2 NVARCHAR(250)
	,in par3 NVARCHAR(250)
	,in par4 NVARCHAR(250)
	,in par5 NVARCHAR(250)
)
AS
BEGIN
	IF :DTYPE='Header' THEN
		SELECT
			  ROW_NUMBER ( ) OVER( ORDER BY A."DocNum" DESC ) AS ROWNUM
			 ,TO_VARCHAR(A."DocNum") AS "DocNum"
			 ,AA."ItemCode" AS "ItemCode"
			 ,C."ItemName" AS "ItemName"
			 ,TO_VARCHAR(A."DocDate",'DD-MM-YYYY') AS "RECDate"
			 ,TO_VARCHAR(E."MnfDate",'DD-MM-YYYY') AS "MFGDate"
			 ,TO_VARCHAR(A."TaxDate",'DD-MM-YYYY') AS "DueDate"
			 ,'SOD : ' || IFNULL( '(W)' || C."U_Width" || IFNULL(' X (L)','') || C."U_Length",'') AS "ProductSize"
			 ,C."U_Pap_Grade" AS "Grade"
			 ,C."U_Flute" AS "FluteType"
			 ,A."CardName" AS "Customer"
			 ,IFNULL(D."Quantity",AA."Quantity") AS "Quantity"
			 ,G."UomCode" AS "UomCode"
			 ,'' AS "RouteStage" -- need p wut to ask customer (note date 01 10 2024)
			 ,A."DocNum" AS "OrderNo"
			 ,C."U_Comb" AS "Combination"
		FROM TRIWALL_TRAINKEY."OPDN" AS A
		LEFT JOIN TRIWALL_TRAINKEY."PDN1" AS AA ON AA."DocEntry"=A."DocEntry"
		LEFT JOIN TRIWALL_TRAINKEY."OITM" AS C ON C."ItemCode"=AA."ItemCode"
		LEFT JOIN TRIWALL_TRAINKEY."IBT1" AS D ON D."ItemCode"=AA."ItemCode" 
											AND D."BaseEntry"=A."DocEntry" 
											AND D."BaseType"=A."ObjType"
		LEFT JOIN TRIWALL_TRAINKEY."OBTN" AS E ON E."DistNumber"=D."BatchNum"
		LEFT JOIN TRIWALL_TRAINKEY."OITB" AS F ON F."ItmsGrpCod"=C."ItmsGrpCod"
		LEFT JOIN TRIWALL_TRAINKEY."OUOM" AS G ON G."UomEntry"=C."UgpEntry"
		WHERE A."DocEntry"=:par1;
	END IF;	
END;
