ALTER PROCEDURE "TRIWALL_TRAINKEY"._USP_CALL_WORK_IN_PROCESS_OIGN_LAYOUT_BARCODE(
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
			 ,C."U_Model" AS "Model"
			 ,C."U_Cust_PN" AS "CustomerPN"
			 ,A."CardName" AS "Customer"
			 ,D."Quantity" AS "Quantity"
			 ,G."UomCode" AS "UomCode"
			 ,'' AS "RefNo"
			 --,C."U_Model" AS "Model"
			-- ,C."U_Cust_PN" AS "CustomerPN"
			-- ,'' AS "OrderNo"
			 ,H."RefSONo" AS "RefSO"
			 ,H."ProductionNumber" AS "OrderNo"
			 ,'' AS "RouteStage" -- P wut need to ask customer about this Routestage
		FROM TRIWALL_TRAINKEY."OIGN" AS A
		LEFT JOIN TRIWALL_TRAINKEY."IGN1" AS AA ON AA."DocEntry"=A."DocEntry"
		LEFT JOIN TRIWALL_TRAINKEY."OITM" AS C ON C."ItemCode"=AA."ItemCode"
		LEFT JOIN TRIWALL_TRAINKEY."IBT1" AS D ON D."ItemCode"=AA."ItemCode" 
											AND D."BaseEntry"=A."DocEntry" 
											AND D."BaseType"=A."ObjType"
		LEFT JOIN TRIWALL_TRAINKEY."OBTN" AS E ON E."DistNumber"=D."BatchNum"
		LEFT JOIN TRIWALL_TRAINKEY."OITB" AS F ON F."ItmsGrpCod"=C."ItmsGrpCod"
		LEFT JOIN TRIWALL_TRAINKEY."OUOM" AS G ON G."UomEntry"=C."UgpEntry"
		LEFT JOIN (
			SELECT 
				 T0."OriginNum" AS "RefSONo"
				,T0."DocNum" AS "ProductionNumber" 
				,T0."DocEntry" AS "DocEntry"
				,T0."ObjType" AS "ObjType"
			FROM TRIWALL_TRAINKEY."OWOR" AS T0
		) AS H ON H."DocEntry"=AA."BaseEntry" AND H."ObjType"=AA."BaseType"
		WHERE A."DocEntry"=:par1;
	END IF;	
END;
