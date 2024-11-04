ALTER PROCEDURE "TRIWALL_TRAINKEY"."_USP_CALLTRANS_EWTRANSACTION"(
	in DTYPE NVARCHAR(250)
	,in par1 NVARCHAR(250)
	,in par2 NVARCHAR(250)
	,in par3 NVARCHAR(250)
	,in par4 NVARCHAR(250)
	,in par5 NVARCHAR(250)
)
AS
BEGIN
USING SQLSCRIPT_STRING AS LIBRARY;
	IF :DTYPE='GET_Purchase_Order_Detail_By_DocNum' THEN
		SELECT 
			 A."CardCode" AS "VendorCode"
			,A."CardName" AS "VendorName"
			,C."Name" AS "ContactPerson"
			,A."CntctCode" AS "ContactPersonId"
			,A."NumAtCard" AS "VendorNo"
			,A."DocNum" AS "Remarks"
			,D."Name" AS "BranchName"
			,IFNULL(A."Branch",1) AS "BranchId"
			,A."DocEntry" AS "BaseDocEntry"
			,B."LineNum" AS "BaseLineNumber"
			,B."ItemCode" AS "ItemCode"
			,B."Dscription" AS "ItemName"
			,CASE WHEN (B."OpenQty"-0/*IFNULL(B."U_oDraftQty",0)*/)>0 
				THEN 
					B."OpenQty"--(B."OpenQty"-IFNULL(B."U_oDraftQty",0)) 
			 ELSE 
			 	B."OpenQty" 
			 END AS "Qty"
			,B."Price" AS "Price"
			,B."LineTotal" AS "LineTotal"
			,B."VatGroup" AS "VatCode"
			,B."UomCode"
			,B."WhsCode" AS "WarehouseCode"
			,E."CodeBars" AS "BarCode"
			,CASE WHEN E."ManBtchNum"='Y' THEN
				'B'
			 WHEN E."ManSerNum"='Y' THEN
				'S'
			 ELSE
				'N'
			 END AS "ManageItem"
		FROM TRIWALL_TRAINKEY."OPOR" AS A
		LEFT JOIN TRIWALL_TRAINKEY."POR1" AS B ON A."DocEntry"=B."DocEntry"
		LEFT JOIN TRIWALL_TRAINKEY."OCPR" AS C ON A."CardCode"=C."CardCode" AND A."CntctCode"=C."CntctCode"
		LEFT JOIN TRIWALL_TRAINKEY."OUBR" AS D ON D."Code"=A."Branch"
		LEFT JOIN TRIWALL_TRAINKEY."OITM" AS E ON E."ItemCode"=B."ItemCode"
		WHERE 
				A."DocNum"=:par2 
			And B."LineStatus"='O';
			--And IFNULL(B."U_oDraftStatus",'')<>'C'
	ELSE IF :DTYPE='GET_GoodReceipt_PO' THEN
		SELECT 
			 "DocNum" AS "DocNum"
			,"CardCode" AS "VendorCode"
			,"CardName" AS "VendorName"
			,"NumAtCard" AS "VendorRefNo"
			,TO_VARCHAR("DocDate",'dd-MMM-yyyy') AS "PostingDate"
			,"DocTotal" AS "DocTotal" 
		FROM TRIWALL_TRAINKEY."OPDN" 
		WHERE 
				"Series"=Case When :par1='' Then "Series" Else :par1 End 
			AND "DocNum" LIKE '%' || CASE WHEN :par2='-1' THEN '' ELSE :par2 END || '%'
			--AND "U_WebID"<>'' 
			And "DocStatus"='O'
			ORDER BY "DocNum" DESC;
	ELSE IF :DTYPE='GET_Good_Reciept_Count' THEN
		SELECT 
			COUNT("DocNum") AS "Count" 
		FROM TRIWALL_TRAINKEY."OPDN" 
		WHERE 
				"Series"=Case When :par1='' Then "Series" Else :par1 End 
			AND "DocNum" LIKE '%'|| CASE WHEN :par2='-1' THEN '' ELSE :par2 END ||'%'
			--AND "U_WebID"<>'' 
			And "DocStatus"='O';
	ELSE IF :DTYPE='GET_GoodReceipt_PO_Header_Detail_By_DocNum' THEN
		SELECT 
			 F."SeriesName" AS "SeriesName"
			,A."DocNum" AS "DocNum"
			,TO_VARCHAR(A."DocDate",'yyyy-mm-dd') AS "DocDate"
			,TO_VARCHAR(A."TaxDate",'yyyy-mm-dd') AS "TaxDate"
			,A."CardCode"|| ' - ' || A."CardName" AS "Vendor"
			,IFNULL(C."Name",'') AS "ContactPerson"
			,IFNULL(A."NumAtCard",'') AS "RefInv"
			,A."DocEntry" AS "DocEntry"
		FROM TRIWALL_TRAINKEY."OPDN" AS A
		LEFT JOIN TRIWALL_TRAINKEY."OCPR" AS C ON A."CardCode"=C."CardCode" AND A."CntctCode"=C."CntctCode"
		LEFT JOIN TRIWALL_TRAINKEY."NNM1" AS F ON F."Series"=A."Series"
		WHERE 
			A."DocEntry"=:par1;
	ELSE IF :DTYPE='GET_PurchaseOrder_Header_Detail_By_DocNum' THEN
		SELECT 
			 F."SeriesName" AS "SeriesName"
			,A."DocNum" AS "DocNum"
			,TO_VARCHAR(A."DocDate",'yyyy-mm-dd') AS "DocDate"
			,TO_VARCHAR(A."TaxDate",'yyyy-mm-dd') AS "TaxDate"
			,A."CardCode" AS "Vendor"
			,IFNULL(C."Name",'') AS "ContactPerson"
			,IFNULL(A."NumAtCard",'') AS "RefInv"
		FROM TRIWALL_TRAINKEY."OPOR" AS A
		LEFT JOIN TRIWALL_TRAINKEY."OCPR" AS C ON A."CardCode"=C."CardCode" AND A."CntctCode"=C."CntctCode"
		LEFT JOIN TRIWALL_TRAINKEY."NNM1" AS F ON F."Series"=A."Series"
		WHERE 
			A."DocEntry"=:par1;
	ELSE IF :DTYPE='GET_GoodReceipt_PO_Line_Detail_By_DocNum' THEN
		SELECT 
			 B."LineNum" AS "BaseLineNumber"
			,B."DocEntry" AS "DocEntry"
			,B."ItemCode" AS "ItemCode"
			,B."Dscription" AS "ItemName"
			,B."OpenQty" AS "Qty"
			,B."Price" AS "Price"
			,B."LineTotal" AS "LineTotal"
			,B."TaxCode" AS "VatCode"
			,B."WhsCode" AS "WarehouseCode"
			,E."CodeBars" AS "BarCode"
			,CASE WHEN E."ManBtchNum"='Y' THEN
				'B'
			 WHEN E."ManSerNum"='Y' THEN
				'S'
			 ELSE
				'N'
			 END AS "ManageItem"
		FROM TRIWALL_TRAINKEY."OPDN" AS A
		LEFT JOIN TRIWALL_TRAINKEY."PDN1" AS B ON A."DocEntry"=B."DocEntry"
		LEFT JOIN TRIWALL_TRAINKEY."OCPR" AS C ON A."CardCode"=C."CardCode" AND A."CntctCode"=C."CntctCode"
		LEFT JOIN TRIWALL_TRAINKEY."OITM" AS E ON E."ItemCode"=B."ItemCode"
		WHERE A."DocEntry"=:par1;
	
	ELSE IF :DTYPE='GetBatchSerialGoodReceipt' THEN
		SELECT ROW_NUMBER() OVER(ORDER BY "ItemCode") AS "LineNum",* FROM (
			SELECT 
				 A."ItemCode" AS "ItemCode"	
				,C."Quantity" AS "Qty"
				,B."DistNumber" AS "SerialBatch"
				,B."MnfSerial" AS "MfrSerialNo"
				,TO_VARCHAR(B."ExpDate",'dd-MM-yyyy') AS "ExpDate"
				,B."MnfDate" AS "MrfDate"
				,'Serial' AS "Type"
			FROM TRIWALL_TRAINKEY."SRI1" AS A
			LEFT JOIN TRIWALL_TRAINKEY."OSRN" AS B ON A."ItemCode"=B."ItemCode" AND B."SysNumber"=A."SysSerial"
			LEFT JOIN TRIWALL_TRAINKEY."OSRI" AS C On C."ItemCode"=A."ItemCode" And C."SysSerial"=A."SysSerial"
			WHERE A."BaseEntry"=:par1 
				AND A."BaseType"=20 
				--And C."Status"<>0
					
			UNION ALL
			
			SELECT 
				 A."ItemCode" AS "ItemCode"	
				,C."Quantity" AS "Qty"
				,B."DistNumber" AS "SerialBatch"
				,B."MnfSerial" AS "MfrSerialNo"
				,TO_VARCHAR("ExpDate",'dd-MM-yyyy') AS "ExpDate"
				,B."MnfDate" AS "MrfDate"
				,'Batch' AS "Type"
			FROM TRIWALL_TRAINKEY."IBT1" AS A 
			LEFT JOIN TRIWALL_TRAINKEY."OBTN" AS B ON A."ItemCode"=B."ItemCode" AND A."BatchNum"=B."DistNumber"
			LEFT JOIN TRIWALL_TRAINKEY."OBTQ" AS C ON C."ItemCode"=A."ItemCode" and B."SysNumber"=C."SysNumber"
			WHERE A."BaseEntry"=:par1
				AND A."BaseType"=20
		)AS A;
	ELSE IF :DTYPE='BatchDetialGoodReceipt' THEN
		--int docNum, string serialBatch
		SELECT 
			 B."DistNumber" AS "SerialBatch"
			,B."LotNumber" AS "LotNo"
			,B."ExpDate" AS "ExpDate"
			,B."Quantity" AS "Qty"
		FROM TRIWALL_TRAINKEY."SRI1" AS A
		LEFT JOIN TRIWALL_TRAINKEY."OSRN" AS B ON A."ItemCode"=B."ItemCode" AND B."SysNumber"=A."SysSerial"
		WHERE A."BaseNum"=:par1 AND B."DistNumber"=:par2 AND A."BaseType"=20
		UNION ALL
		SELECT 
			 A."BatchNum"
			,B."LotNumber"
			,B."ExpDate"
			,A."Quantity"
		FROM TRIWALL_TRAINKEY."IBT1" AS A 
		LEFT JOIN TRIWALL_TRAINKEY."OBTN" AS B ON A."ItemCode"=B."ItemCode" AND A."BatchNum"=B."DistNumber"
		WHERE A."BaseNum"=:par1 AND B."DistNumber"=:par2  AND A."BaseType"=20;
	ELSE IF :DTYPE='GET_SALE_ORDER' THEN
		SELECT 
			 "DocNum" AS "DocNum"
			,"CardCode" AS "VendorCode"
			,"CardName" AS "VendorName"
			,"NumAtCard" AS "VendorRefNo"
			,TO_VARCHAR("DocDate",'dd-MM-yyyy') AS "PostingDate"
			,"DocTotal" AS "DocTotal" 	
		FROM TRIWALL_TRAINKEY."ORDR" 
		WHERE 
			"Series"=Case When :par1='-1' Then "Series" Else :par1 End  
			AND "DocNum" LIKE '%'|| :par2 ||'%' 
			AND "DocStatus" ='O'
			ORDER BY "DocNum" DESC;
			/*OFFSET cast(@par3 as int) ROWS
				FETCH NEXT cast(@par4 as int) ROWS ONLY*/
	ELSE IF :DTYPE='GET_SALE_ORDER_Count' THEN
		SELECT 
			COUNT("DocNum") AS "Count" 
		FROM TRIWALL_TRAINKEY."ORDR" 
		WHERE 
			"Series"=Case When :par1='-1' Then "Series" Else :par1 End  
			AND "DocNum" LIKE '%'|| :par2 || '%' 
			AND "DocStatus" ='O';
	ELSE IF :DTYPE='GET_Sale_Order_Detail_By_DocNum' THEN
		SELECT 
			 A."CardCode" AS "VendorCode"
			,A."CardName" AS "VendorName"
			,C."Name" AS "ContactPerson"
			,IFNULL(A."CntctCode" ,'')AS "ContactPersonId"
			,A."NumAtCard" AS "VendorNo"
			,A."DocNum" AS "Remarks"
			,A."BPLName" AS "BranchName"
			,A."BPLId" AS "BranchId"
			,A."DocEntry" AS "BaseDocEntry"
			,B."LineNum" AS "BaseLineNumber"
			,B."ItemCode" AS "ItemCode"
			,B."Dscription" AS "ItemName"
			,B."OpenQty" AS "Qty"
			,B."Price" AS "Price"
			,B."LineTotal" AS "LineTotal"
			,B."TaxCode" AS "VatCode"
			,B."WhsCode" AS "WarehouseCode"
			,'' /*A."U_DeliCon"*/ AS "DeliveryCondition"
			,CASE WHEN E."ManBtchNum"='Y' THEN
				'B'
			 WHEN E."ManSerNum"='Y' THEN
				'S'
			 ELSE
				'N'
			 END AS "ManageItem"
			,A."DocStatus" AS "Status"
			,E."CodeBars" AS "BarCode"
		FROM TRIWALL_TRAINKEY."ORDR" AS A
		LEFT JOIN TRIWALL_TRAINKEY."RDR1" AS B ON A."DocEntry"=B."DocEntry"
		LEFT JOIN TRIWALL_TRAINKEY."OCPR" AS C ON A."CardCode"=C."CardCode" AND A."CntctCode"=C."CntctCode"
		--LEFT JOIN OUBR AS D ON D.Code=A.Branch
		LEFT JOIN TRIWALL_TRAINKEY."OITM" AS E ON E."ItemCode"=B."ItemCode"
		WHERE 
			A."CardCode"=CASE WHEN :par1='' THEN A."CardCode" ELSE :par1 END 
			AND A."DocNum"=:par2 
			AND B."LineStatus"= 'O';  
	ELSE IF :DTYPE='GET_Delivery' THEN
		SELECT 
			 "DocNum" AS "DocNum"
			,"DocEntry" AS "DocEntry"
			,"CardCode" AS "VendorCode"
			,"CardName" AS "VendorName"
			,"DocTotal" AS "DocTotal" 
			,"NumAtCard" AS "VendorRefNo"
			,TO_VARCHAR("DocDate",'dd-MM-yyyy') AS "PostingDate"
		FROM TRIWALL_TRAINKEY."ODLN" 
		WHERE 
			"Series"=case when :par1='' Then "Series" Else :par1 End 
			AND "DocStatus"='O'
			AND "DocNum" LIKE '%' || :par2 || '%'
			ORDER BY "DocNum" DESC;
			/*
				AND DocNum = Case when @par2='' Then DocNum Else Cast(@par2 AS INT) End
				AND U_WebID<>'' And ISNULL(U_docDraft,'')<>'oDraft' 
				OFFSET cast(@par3 as int) ROWS
				FETCH NEXT cast(@par4 as int) ROWS ONLY;*/
	ELSE IF :DTYPE='GET_Reutrn_Count' THEN
		SELECT 
			COUNT("DocNum") AS "Count" 
		FROM TRIWALL_TRAINKEY."ODLN" 
		WHERE 
			"Series"=case when :par1='' Then "Series" Else :par1 End 
			AND "DocStatus"='O'
			AND "DocNum" LIKE '%' || :par2 || '%';
			/*
				AND U_WebID<>'' And ISNULL(U_docDraft,'')<>'oDraft'
				AND DocNum = Case when @par2='' Then DocNum Else Cast(@par2 AS INT) End
			*/ 
	ELSE IF :DTYPE='GET_Delivery_Detail_By_DocNum' THEN
		SELECT 
			 A."CardCode" AS "VendorCode"
			,A."CardName" AS "VendorName"
			,C."Name" AS "ContactPerson"
			,A."CntctCode" AS "ContactPersonId"
			,A."NumAtCard" AS "VendorNo"
			,A."DocNum" AS "Remarks"
			,A."BPLName" AS "BranchName"
			,A."BPLId" AS "BranchId"
			,A."DocEntry" AS "DocEntry"
			,B."LineNum" AS "BaseLineNumber"
			,B."ItemCode" AS "ItemCode"
			,B."Dscription" AS "ItemName"
			,B."OpenQty"-0/*IFNULL("U_oDraftQty",0)*/ AS "Qty"
			,B."Price" AS "Price"
			,B."LineTotal" AS "LineTotal"
			,B."TaxCode" AS "VatCode"
			,B."WhsCode" AS "WarehouseCode"
			,B."CodeBars" AS "BarCode"
			,/*A."U_DeliCon"*/'' AS DeliveryCondition
			,CASE WHEN E."ManBtchNum"='Y' THEN
				'B'
			 WHEN E."ManSerNum"='Y' THEN
				'S'
			 ELSE
				'N'
			 END AS "ManageItem"
		FROM TRIWALL_TRAINKEY."ODLN" AS A
		LEFT JOIN TRIWALL_TRAINKEY."DLN1" AS B ON A."DocEntry"=B."DocEntry"
		LEFT JOIN TRIWALL_TRAINKEY."OCPR" AS C ON A."CardCode"=C."CardCode" AND A."CntctCode"=C."CntctCode"
		LEFT JOIN TRIWALL_TRAINKEY."OUBR" AS D ON D."Code"=A."Branch"
		LEFT JOIN TRIWALL_TRAINKEY."OITM" AS E ON E."ItemCode"=B."ItemCode"
		WHERE 
			A."DocNum"=:par2 
			AND B."LineStatus"= 'O';
			/*And IFNULL("U_oDraftStatus",'')<>'C'*/
	ELSE IF :DTYPE='GetBatchSerialDelivery' THEN
		--nt docNum, string itemCode, string itemType
		IF :par3='S' THEN
			DECLARE DocEntry INT;
			CREATE LOCAL TEMPORARY TABLE #TMPDLN(ItemCode NVARCHAR(255),"U_OutSerial" NVARCHAR(255));
			SELECT "DocEntry" INTO DocEntry From TRIWALL_TRAINKEY."ODLN" Where "DocNum"=:par1;
			SELECT "ItemCode",''/*"U_OutSerial"*/ FROM TRIWALL_TRAINKEY."DLN1" WHERE "DocEntry"=:DocEntry INTO #TMPDLN;

			SELECT 
				 B."DistNumber" AS "SerialBatch"
				,B."LotNumber" AS "LotNo"
				,B."ExpDate" AS "ExpDate"
				,1 As "Qty"
				,B."SysNumber" AS "SysNumber"
				,B."ItemCode" AS "ItemCode"
				
			FROM TRIWALL_TRAINKEY."SRI1" AS A
			LEFT JOIN TRIWALL_TRAINKEY."OSRN" AS B ON A."ItemCode"=B."ItemCode" AND B."SysNumber"=A."SysSerial"
			LEFT JOIN TRIWALL_TRAINKEY."OSRI" AS C ON C."ItemCode"=A."ItemCode" And C."SysSerial"=A."SysSerial"
			WHERE 
				A."BaseNum"=:par1 AND A."ItemCode"=:par2 AND A."BaseType"=15 And C."Status"<>0 
				AND B."DistNumber" 
					NOT IN (
						SELECT 
							B."DistNumber"
						FROM TRIWALL_TRAINKEY."SRI1" A
						LEFT JOIN TRIWALL_TRAINKEY."OSRN" B ON A."ItemCode"=B."ItemCode" And A."SysSerial"=B."SysNumber"
						LEFT JOIN TRIWALL_TRAINKEY."OSRI" AS C ON C."ItemCode"=A."ItemCode" And C."SysSerial"=A."SysSerial"
						WHERE  
							A."BaseType"=16 
							And  C."Status"<>0 
							And  A."ItemCode"=:par2 
							ANd A."BsDocEntry"=(Select "DocEntry" From TRIWALL_TRAINKEY."ODLN" WHERE "DocNum"=:par1)
					);
			DROP TABLE #TMPDLN;		
		ELSE IF :par3='B' THEN
			Declare DocEntry1 INT;
			create local temporary table #TMPDLN1("ItemCode" NVARCHAR(255),"U_OutSerial" NVARCHAR(255));
			Select "DocEntry" INTO DocEntry1 From TRIWALL_TRAINKEY."ODLN" Where "DocNum"=:par1;
			SELECT "ItemCode",''/*"U_OutSerial"*/ FROM TRIWALL_TRAINKEY."DLN1" WHERE "DocEntry"=:DocEntry1 INTO #TMPDLN1;

			SELECT * FROM (
				SELECT 
					 A."BatchNum" AS "SerialBatch"
					,B."LotNumber" AS "LotNo"
					,B."ExpDate" AS "ExpDate"
					,A."Quantity"
						-
					IFNULL(
						(SELECT 
							SUM(Z."Quantity") 
						 FROM TRIWALL_TRAINKEY."IBT1" Z 
						 WHERE 
							 "BaseType"=16 
							 AND Z."BatchNum"=A."BatchNum" 
							 AND Z."ItemCode"=A."ItemCode" 
							 AND Z."BsDocEntry"=A."BaseEntry" 
							 AND Z."WhsCode"=A."WhsCode"
						),0) AS "Qty"
					,B."SysNumber" AS "SysNumber"
					,B."ItemCode" AS "ItemCode"
				FROM TRIWALL_TRAINKEY."IBT1" AS A 
				LEFT JOIN TRIWALL_TRAINKEY."OBTN" AS B ON A."ItemCode"=B."ItemCode" AND A."BatchNum"=B."DistNumber"
				WHERE A."BaseNum"=:par1 AND A."ItemCode"=:par2 AND A."BaseType"=15
			)A wHERE "Qty"<>0 ;
			DROP TABLE #TMPDLN1;
		END IF;
		END IF;
	ELSE IF :DTYPE='BatchDetialDelivery' THEN
		--int docNum, string serialBatch
		SELECT 
			 B."DistNumber" AS "SerialBatch"
			,B."LotNumber" AS "LotNo"
			,B."ExpDate" AS "ExpDate"
			,B."Quantity" AS "Qty"
		FROM TRIWALL_TRAINKEY."SRI1" AS A
		LEFT JOIN TRIWALL_TRAINKEY."OSRN" AS B ON A."ItemCode"=B."ItemCode" AND B."SysNumber"=A."SysSerial"
		WHERE A."BaseNum"=:par1 AND B."DistNumber"=:par2 AND A."BaseType"=15
		UNION ALL
		SELECT 
			 A."BatchNum"
			,B."LotNumber"
			,B."ExpDate"
			,A."Quantity"
		FROM TRIWALL_TRAINKEY."IBT1" AS A 
		LEFT JOIN TRIWALL_TRAINKEY."OBTN" AS B ON B."ItemCode"=A."ItemCode" AND B."DistNumber"=A."BatchNum"
		WHERE A."BaseNum"=:par1 AND B."DistNumber"=:par2  AND A."BaseType"=15;
	ELSE IF :DTYPE='BatchOrSerial' THEN
		IF :par1='S' THEN
			SELECT 
				 T0."ItemCode"
				,T5."DistNumber" AS "SerialOrBatch"
				,CAST(T3."OnHandQty" AS INT) AS "Qty"
				,T5."SysNumber"
				,TO_VARCHAR(T5."InDate",'dd-MM-yyyy') AS "AdmissionDate"
				,IFNULL(TO_VARCHAR(T4."ExpDate",'dd-MM-yyyy'),'') AS "ExpDate"
				,IFNULL(T4."LotNumber",'') AS "LotNumber"
			FROM TRIWALL_TRAINKEY."OIBQ" T0
			INNER JOIN TRIWALL_TRAINKEY."OBIN" T1 on T0."BinAbs" = T1."AbsEntry" AND T0."OnHandQty" <> 0
			LEFT OUTER JOIN TRIWALL_TRAINKEY."OBBQ" T2 on T0."BinAbs" = T2."BinAbs" AND T0."ItemCode" = T2."ItemCode" AND T2."OnHandQty" <> 0
			LEFT OUTER JOIN TRIWALL_TRAINKEY."OSBQ" T3 on T0."BinAbs" = T3."BinAbs" AND T0."ItemCode" = T3."ItemCode" AND T3."OnHandQty" <> 0
			LEFT OUTER JOIN TRIWALL_TRAINKEY."OBTN" T4 on T2."SnBMDAbs" = T4."AbsEntry" AND T2."ItemCode" = T4."ItemCode"
			LEFT OUTER JOIN TRIWALL_TRAINKEY."OSRN" T5 on T3."SnBMDAbs" = T5."AbsEntry" AND T3."ItemCode" = T5."ItemCode"
			WHERE
				T1."AbsEntry" >= 0
				AND (T3."AbsEntry" is not null)
				AND T0."ItemCode" in(
					(
						SELECT 
							U0."ItemCode" 
						FROM TRIWALL_TRAINKEY."OITM" U0 
						INNER JOIN TRIWALL_TRAINKEY."OITB" U1 ON U0."ItmsGrpCod" = U1."ItmsGrpCod"
						WHERE U0."ItemCode" IS NOT NULL ))
				AND T1."WhsCode" IN	(SELECT * FROM LIBRARY:SPLIT_TO_TABLE(:par3,','))
				AND T0."ItemCode" IN (SELECT * FROM LIBRARY:SPLIT_TO_TABLE(:par2,','));
		ELSE IF :par1='B' THEN
			SELECT 
				 A."ItemCode" AS "ItemCode"
				,A."DistNumber" AS "SerialOrBatch"
				,B."Quantity" AS "Qty"
				,A."SysNumber" AS "SysNumber"
				,TO_VARCHAR(A."ExpDate",'dd-MM-yyyy') AS "ExpDate"
				,A."LotNumber" AS "LotNumber"
				,A."InDate" AS "AdmissionDate"
				,IFNULL(TO_VARCHAR(A."ExpDate",'dd-MM-yyyy'),'') AS "ExpDate"
				,IFNULL(A."LotNumber",'') AS "LotNumber"
			FROM TRIWALL_TRAINKEY."OBTN" AS A
			LEFT JOIN TRIWALL_TRAINKEY."OBTQ" AS B ON A."ItemCode"=B."ItemCode" AND A."SysNumber"=B."SysNumber"
			WHERE 
				A."ItemCode"=:par2 
				AND B."Quantity" !=0 
				And B."WhsCode"=:par3;
		END IF;
		END IF;
	ELSE IF :DTYPE='GET_Inventory_StockCount' THEN
		SELECT 
			 "DocNum" AS "DocNum"
			,A."CountDate" AS "CounterDate"
			,CASE WHEN A."CountType"='1' THEN 
				'Single Counter' 
			 WHEN A."CountType"='2' THEN 
			 	'Multiple Counters' 
			 END AS "CounterType"
			,CASE WHEN LENGTH(A."Time")=3 THEN 
				LEFT(A."Time",1)+':'+RIGHT(A."Time",2) 
			 ELSE 
			 	LEFT(A."Time",2)+':'+RIGHT(A."Time",2) 
			 END AS "CountTime" 
		FROM TRIWALL_TRAINKEY."OINC" AS A 
		LEFT JOIN TRIWALL_TRAINKEY."INC8" AS B ON A."DocEntry"=B."DocEntry" 
		WHERE 
			"Series"=:par1 
			AND B."CounterId"=:par3 
			AND "DocNum" LIKE '%'|| :par2 ||'%';
	ELSE IF :DTYPE='GET_Inventory_StockCount_Detail_ByDocNum' THEN
		SELECT 
			 A."CountDate" AS "CountDate"
			,A."Series" AS "Series"
			,A."DocNum" AS "DocNum"
			,CASE WHEN A."CountType"='1' THEN 'Single Counter' ELSE 'Multiple Counters' END AS "CountType"
			,A."IndvCount" AS "IndvCount"
			,A."TeamCount" AS "TeamCount"
			,A."Status" AS "Status"
			,A."Ref2" AS "Ref"
			,A."Remarks" AS "Remarks"
			,B."ItemCode" AS "ItemCode"
			,B."ItemDesc" AS "ItemName"
			,B."InWhsQty" AS "Qty"
			,B."WhsCode" AS "WarehouseCode"
			,F."TotalQty" AS "QtyCounted"
			,B."Counted" AS "Counted"
			,B."LineNum" AS "LineNum"
			,CASE WHEN E."ManBtchNum"='Y' THEN
				'B'
			 WHEN E."ManSerNum"='Y' THEN
				'S'
			 ELSE
				'N'
			 END AS "ManageItem"
			,A."DocEntry" AS "DocEntry"
		FROM TRIWALL_TRAINKEY."OINC" AS A
		LEFT JOIN TRIWALL_TRAINKEY."INC1" AS B ON A."DocEntry"=B."DocEntry"
		LEFT JOIN TRIWALL_TRAINKEY."OITM" AS E ON E."ItemCode"=B."ItemCode"
		LEFT JOIN (
			SELECT 
				 INC9."DocEntry"
				,INC8."CounterId"
				,"TotalQty" 
			FROM TRIWALL_TRAINKEY."INC9" 
			LEFT JOIN TRIWALL_TRAINKEY."INC8" ON INC8."CounterNum"=INC9."CounterNum" AND INC8."DocEntry"=INC9."DocEntry"
		)As F ON A."DocEntry"=F."DocEntry" AND F."CounterId"=:par2
		WHERE  A."DocNum"=:par1;
	ELSE IF :DTYPE='Number_of_Individual_Counter' THEN
		SELECT 
			 "CounterId" AS "CounterNum"
			,"CounteName" AS "CounterName" 
		FROM TRIWALL_TRAINKEY."INC8" 
		WHERE "DocEntry"=:par1;
	ELSE IF :DTYPE='Number_of_Multiple_Counter' THEN
		SELECT 
			 IFNULL("CounterNum",0) AS "TeamCounterNum"
			,IFNULL("CounteName",'') AS "TeamCounterName" 
		FROM TRIWALL_TRAINKEY."INC4" 
		WHERE "DocEntry"=:par1;
	ELSE IF :DTYPE='SERIES' THEN
		IF :par1='59' THEN
			SELECT 
				 "Series" AS "Code"
				,"SeriesName"
				,(SELECT 
					IFNULL(MAX("DocNum")+1,B."InitialNum") 
				  FROM TRIWALL_TRAINKEY."OIGN" 
				  WHERE "Series"=B."Series"
				 )AS "DocNum"
			FROM TRIWALL_TRAINKEY."OFPR" AS A 
			LEFT JOIN TRIWALL_TRAINKEY."NNM1" AS B ON A."Indicator"=B."Indicator"
			WHERE 
				A."Category"=YEAR(CURRENT_DATE)
				AND TO_VARCHAR(B."ObjectCode")=59 
				AND "SubNum"=MONTH(CURRENT_DATE) 
				And B."Indicator"<>'Default';
		ELSE IF :par1='60' THEN
			SELECT 
				 "Series" AS "Code"
				,"SeriesName"
				,(SELECT 
					IFNULL(MAX("DocNum")+1,B."InitialNum") 
				  FROM TRIWALL_TRAINKEY."OIGE" 
				  WHERE "Series"=B."Series"
				 )AS "DocNum"
			FROM TRIWALL_TRAINKEY."OFPR" AS A 
			LEFT JOIN TRIWALL_TRAINKEY."NNM1" AS B ON A."Indicator"=B."Indicator"
			WHERE 
				A."Category"=YEAR(CURRENT_DATE)
				AND TO_VARCHAR(B."ObjectCode")=60 
				AND "SubNum"=MONTH(CURRENT_DATE) 
				And B."Indicator"<>'Default';
		ELSE IF :par1='22' THEN
			SELECT 
				 "Series" AS "Code"
				,"SeriesName"
				,(SELECT 
					IFNULL(MAX("DocNum")+1,B."InitialNum") 
				  FROM TRIWALL_TRAINKEY."OPOR" 
				  WHERE "Series"=B."Series"
				 )AS "DocNum" 
			FROM TRIWALL_TRAINKEY."OFPR" AS A 
			LEFT JOIN TRIWALL_TRAINKEY."NNM1" AS B ON A."Indicator"=B."Indicator"
			WHERE 
				B."Indicator"=YEAR(CURRENT_DATE) 
				AND B."ObjectCode"=22 
				AND "SubNum"=MONTH(CURRENT_DATE) 
				And B."Indicator"<>'Default';
		ELSE IF :par1='20' THEN
			SELECT 
				 "Series" AS "Code"
				,"SeriesName"
				,(SELECT 
					IFNULL(MAX("DocNum")+1,B."InitialNum") 
				  FROM TRIWALL_TRAINKEY."OPDN" 
				  WHERE "Series"=B."Series"
				 )AS "DocNum"
			FROM TRIWALL_TRAINKEY."OFPR" AS A 
			LEFT JOIN TRIWALL_TRAINKEY."NNM1" AS B ON A."Indicator"=B."Indicator"
			WHERE 
				A."Category"=YEAR(CURRENT_DATE)
				AND TO_VARCHAR(B."ObjectCode")=20 
				AND "SubNum"=MONTH(CURRENT_DATE) 
				And B."Indicator"<>'Default';
		ELSE IF :par1='21' THEN
			SELECT 
				 "Series" AS "Code"
				,"SeriesName"
				,(SELECT 
					IFNULL(MAX("DocNum")+1,B."InitialNum") 
				  FROM TRIWALL_TRAINKEY."ORPD" 
				  WHERE "Series"=B."Series"
				 )AS "DocNum"
			FROM TRIWALL_TRAINKEY."OFPR" AS A 
			LEFT JOIN TRIWALL_TRAINKEY."NNM1" AS B ON A."Indicator"=B."Indicator"
			WHERE 
				A."Category"=YEAR(CURRENT_DATE)
				AND TO_VARCHAR(B."ObjectCode")=21 
				AND "SubNum"=MONTH(CURRENT_DATE) 
				And B."Indicator"<>'Default';
		ELSE IF :par1='17' THEN
			SELECT 
				 "Series" AS "Code"
				,"SeriesName"
				,B."NextNumber" As "DocNum"
			FROM TRIWALL_TRAINKEY."OFPR" AS A 
			LEFT JOIN TRIWALL_TRAINKEY."NNM1" AS B ON A."Indicator"=B."Indicator"
			WHERE B."Indicator"=YEAR(CURRENT_DATE) AND 
				B."ObjectCode"=17 AND "SubNum"=MONTH(CURRENT_DATE)
				And B."Indicator"<>'Default';
		ELSE IF :par1='15' THEN
			SELECT 
				 "Series" AS "Code"
				,"SeriesName"
				,(SELECT 
					IFNULL(MAX("DocNum")+1,B."InitialNum") 
				  FROM TRIWALL_TRAINKEY."ODLN" 
				  WHERE "Series"=B."Series"
				 )AS "DocNum"
			FROM TRIWALL_TRAINKEY."OFPR" AS A 
			LEFT JOIN TRIWALL_TRAINKEY."NNM1" AS B ON A."Indicator"=B."Indicator"
			WHERE 
				A."Category"=YEAR(CURRENT_DATE)
				AND TO_VARCHAR(B."ObjectCode")=15 
				AND "SubNum"=MONTH(CURRENT_DATE) 
				And B."Indicator"<>'Default';
		ELSE IF :par1='16' THEN
			SELECT 
				 "Series" AS "Code"
				,"SeriesName"
				,(SELECT 
					IFNULL(MAX("DocNum")+1,B."InitialNum") 
				  FROM TRIWALL_TRAINKEY."ORDN" 
				  WHERE "Series"=B."Series"
				 )AS "DocNum"
			FROM TRIWALL_TRAINKEY."OFPR" AS A 
			LEFT JOIN TRIWALL_TRAINKEY."NNM1" AS B ON A."Indicator"=B."Indicator"
			WHERE 
				A."Category"=YEAR(CURRENT_DATE)
				AND TO_VARCHAR(B."ObjectCode")=16 
				AND "SubNum"=MONTH(CURRENT_DATE) 
				And B."Indicator"<>'Default';
		ELSE IF :par1='1250000001' THEN
		--Inventory Transfer Request
			SELECT 
				 B."Series" AS "Code"
				,"SeriesName"
				,B."BPLId" As "Branch"
				,(SELECT 
					IFNULL(MAX("DocNum")+1,B."InitialNum") 
				  FROM TRIWALL_TRAINKEY."OWTQ" 
				  WHERE "Series"=B."Series"
				 ) AS DocNum 
			FROM TRIWALL_TRAINKEY."OFPR" AS A 
			LEFT JOIN TRIWALL_TRAINKEY."NNM1" AS B ON B."Indicator"=A."Indicator"
			WHERE 
				B."Indicator"=YEAR(CURRENT_DATE) 
				AND B."ObjectCode"=1250000001 
				AND A."SubNum"=MONTH(CURRENT_DATE) 
				AND B."Indicator"<>'Default';
		ELSE IF :par1='67' THEN
		--Inventory Transfer Request
			SELECT 
				 "Series" AS "Code"
				,"SeriesName"
				,(SELECT 
					IFNULL(MAX("DocNum")+1,B."InitialNum") 
				  FROM TRIWALL_TRAINKEY."OWTR" 
				  WHERE "Series"=B."Series"
				 )AS "DocNum"
			FROM TRIWALL_TRAINKEY."OFPR" AS A 
			LEFT JOIN TRIWALL_TRAINKEY."NNM1" AS B ON A."Indicator"=B."Indicator"
			WHERE 
				A."Category"=YEAR(CURRENT_DATE)
				AND TO_VARCHAR(B."ObjectCode")=67 
				AND "SubNum"=MONTH(CURRENT_DATE) 
				And B."Indicator"<>'Default';
		ELSE IF :par1='1470000065' THEN
			SELECT 
				 B."Series" AS "Code"
				,"SeriesName"
				,B."BPLId" As "Branch"
				,C."BPLName"
				,(SELECT 
					IFNULL(MAX("DocNum")+1,B."InitialNum") As "DocNum"	
				  FROM TRIWALL_TRAINKEY."OINC" WHERE "Series"=B."Series"
				 )AS "DocNum" 
			FROM TRIWALL_TRAINKEY."OFPR" AS A 
			LEFT JOIN TRIWALL_TRAINKEY."NNM1" AS B ON A."Indicator"=B."Indicator"
			LEFT JOIN TRIWALL_TRAINKEY."OBPL" AS C ON C."BPLId"=B."BPLId"
			WHERE 
				B."Indicator"=YEAR(CURRENT_DATE) 
				AND B."ObjectCode"=1470000065 
				AND A."SubNum"=MONTH(CURRENT_DATE) 
				AND B."Indicator"<>'Default';
		ELSE IF :par1='202' THEN
			SELECT 
				 "Series" AS "Code"
				,"SeriesName"
				,"NextNumber" As "DocNum"
			FROM TRIWALL_TRAINKEY."OFPR" AS A 
			LEFT JOIN TRIWALL_TRAINKEY."NNM1" AS B ON A."Indicator"=B."Indicator"
			WHERE 
				B."Indicator"=YEAR(CURRENT_DATE) 
				AND B."ObjectCode"=202 
				AND "SubNum"=MONTH(CURRENT_DATE) 
				AND B."Indicator"<>'Default';
		ELSE IF :par1='13' THEN
			SELECT 
				 "Series" AS "Code"
				,"SeriesName"
				,A."NextNumber" As "DocNum"
			FROM TRIWALL_TRAINKEY."NNM1"  A 
			WHERE 
				A."Indicator"=YEAR(CURRENT_DATE)
				AND A."ObjectCode"=13 
				AND A."Indicator"<>'Default';
		ELSE IF :par1='14' THEN
			SELECT 
				 "Series" AS "Code"
				,"SeriesName"
				,(SELECT 
					IFNULL(MAX("DocNum")+1,B."InitialNum") 
				  FROM TRIWALL_TRAINKEY."ORIN" 
				  WHERE "Series"=B."Series"
				 )AS "DocNum"
			FROM TRIWALL_TRAINKEY."OFPR" AS A 
			LEFT JOIN TRIWALL_TRAINKEY."NNM1" AS B ON A."Indicator"=B."Indicator"
			WHERE 
				A."Category"=YEAR(CURRENT_DATE)
				AND TO_VARCHAR(B."ObjectCode")=14 
				AND "SubNum"=MONTH(CURRENT_DATE) 
				And B."Indicator"<>'Default';
		ELSE IF :par1='67' THEN
			SELECT 
				 "Series" AS "Code"
				,"SeriesName"
				,A."NextNumber" As "DocNum"
			FROM TRIWALL_TRAINKEY."NNM1" A 
			WHERE 
				A."Indicator"=YEAR(CURRENT_DATE) 
				AND A."ObjectCode"=67 
				AND A."Indicator"<>'Default';
		ELSE IF :par1='234000031' THEN
			SELECT 
				 "Series" AS "Code"
				,"SeriesName"
				,(SELECT 
					IFNULL(MAX("DocNum")+1,B."InitialNum") 
				  FROM TRIWALL_TRAINKEY."ORDN" 
				  WHERE "Series"=B."Series"
				 )AS "DocNum"
			FROM TRIWALL_TRAINKEY."OFPR" AS A 
			LEFT JOIN TRIWALL_TRAINKEY."NNM1" AS B ON A."Indicator"=B."Indicator"
			WHERE 
				A."Category"=YEAR(CURRENT_DATE)
				AND TO_VARCHAR(B."ObjectCode")='234000031' 
				AND "SubNum"=MONTH(CURRENT_DATE) 
				And B."Indicator"<>'Default';
		END IF;
		END IF;
		END IF;
		END IF;
		END IF;
		END IF;
		END IF;
		END IF;
		END IF;
		END IF;
		END IF;
		END IF;
		END IF;
		END IF;
		END IF;
		END IF;
	ELSE IF :DTYPE='GetItemCodeByBarCode' THEN
		SELECT   
			 "ItemCode" AS "ItemCode"
			,"ItemName" AS "ItemName"
			,(SELECT 
				"Price" 
			  FROM TRIWALL_TRAINKEY."ITM1" 
			  WHERE 
			  	"PriceList"=1 
			  	AND "ItemCode"=OITM."ItemCode"
			 ) AS "PriceUnit"
		FROM TRIWALL_TRAINKEY."OITM" 
		WHERE "CodeBars"=:par1;
	ELSE IF :DTYPE='GetVendor' THEN
		SELECT  
			 "CardCode" AS "VendorCode"
			,"CardName" As "VendorName"
			,"Phone1" As "PhoneNumber"
			,"CntctPrsn" as "ContactID"
		FROM TRIWALL_TRAINKEY."OCRD" 
		WHERE "CardType"='S';
	ELSE IF :DTYPE='GetBranch' THEN
		SELECT 
			 "BPLId" AS "BranchID"
			,"BPLName" AS "BranchName"
		FROM TRIWALL_TRAINKEY."OBPL" 
		WHERE "Disabled"!='Y';
	ELSE IF :DTYPE='GetContactPersonByCardCode' THEN
		SELECT  
			"CntctCode" AS "ContactID"
			,"Name" As "ContactName"
			,"CardCode" AS "CardCode"
		FROM TRIWALL_TRAINKEY."OCPR" 
		WHERE "CardCode"=CASE WHEN :par1='' THEN "CardCode" ELSE :par1 END;
	ELSE IF :DTYPE='GetItem' THEN
		SELECT  
			 A."ItemCode" AS "ItemCode"
			,A."ItemName" AS "ItemName"
			,CAST(IFNULL(B."Price",0.00) AS float) AS "PriceUnit"
			,CASE WHEN A."ManSerNum"='Y' THEN
			 	'S'
			 WHEN A."ManBtchNum"='Y' THEN
			 	'B'
			 ELSE 'N' END AS "ItemType"	
		FROM TRIWALL_TRAINKEY."OITM" AS A
		LEFT JOIN TRIWALL_TRAINKEY."ITM1" AS B ON B."ItemCode"=A."ItemCode" AND B."PriceList"=1;
	ELSE IF :DTYPE='GennerateBatchOrSerial' THEN
		SELECT 
			  :par1
			||:par2
			||TO_VARCHAR(CURRENT_DATE,'yyyy')
			||TO_VARCHAR(CURRENT_DATE,'MM')
			||TO_VARCHAR(CURRENT_DATE,'dd')
			||TO_VARCHAR(CURRENT_TIME,'hh:mm:ss') AS BatchOrSerial
		FROM DUMMY;
	ELSE IF :DTYPE='GetWarehouseMasterData' THEN
		SELECT 
			 "WhsCode" AS "Code"
			,"WhsName" AS "Name" 
			,A0."BPLName" AS "BranchName"
		FROM TRIWALL_TRAINKEY."OWHS" AS A
		LEFT JOIN TRIWALL_TRAINKEY."OBPL" AS A0 ON A0."BPLId"=A."BPLid"
		WHERE "Inactive"='N';
	ELSE IF :DTYPE='GetTransferRequest' THEN
		SELECT 
			 "DocNum" AS "DocNum"
			,"CardCode" AS "CardCode"
			,"CardName" AS "CardName"
			,"DocTotal" AS "DocTotal" 
		FROM TRIWALL_TRAINKEY."OPOR" 
		WHERE 
			"Series"=CASE WHEN :par1=0 THEN "Series" ELSE :par1 END 
			AND "DocNum" LIKE '%' || :par2 || '%';
	ELSE IF :DTYPE='GET_Inventory_Transfer_Request' THEN
		SELECT 
			 "DocNum"
			,TO_VARCHAR("DocDate",'yyyy-MM-dd') AS "DocDate"
			,"BPLName" AS "Branch"
			,"Filler" AS "FromWarehouse"
			,"ToWhsCode" AS "ToWarehouse" 
		FROM TRIWALL_TRAINKEY."OWTQ" 
		WHERE 
			"DocStatus"='O' 
			AND "Series"=:par1 
			AND "DocNum" LIKE '%'|| :par2 ||'%';
	ELSE IF :DTYPE='GET_Inventory_Request_DetailByDocNum' THEN
		SELECT
			 TO_VARCHAR(A."DocDate",'yyyy-MM-dd') As "DocDate"
			,TO_VARCHAR("ShipDate",'yyyy-MM-dd') As "ShipDate"			
			,A."CardCode"
			,A."CardName"
			,A."NumAtCard"
			,A."Address"
			,A."JrnlMemo" As "Remark"
			,A."BPLId"
			,A."BPLName"
			,A."Filler" As "HWhsFrom"
			,A."ToWhsCode" As "HWhsTo"
			,A."DocEntry"
			,B."LineNum"
			,B."ItemCode"
			,"Dscription"
			,Cast(B."OpenQty" As INT) As "Qty"
			,B."Price"
			,B."LineTotal"
			,"UomCode"
			,B."FromWhsCod"
			,B."WhsCode"
			,IFNULL(C."ManBtchNum",'')+IFNULL(C."ManSerNum",'') As "ManItem"
			,A."BPLName" As "Branch"
		FROM TRIWALL_TRAINKEY."OWTQ" A
		LEFT JOIN TRIWALL_TRAINKEY."WTQ1" B ON A."DocEntry"=B."DocEntry"
		LEFT JOIN TRIWALL_TRAINKEY."OITM" C ON B."ItemCode"=C."ItemCode"
		WHERE 
			A."DocStatus"='O' 
			AND A."DocNum" IN(:par1) 
			AND B."LineStatus"='O';
	ELSE IF :DTYPE='GET_Production_Order' THEN
		IF :par1='GetForIssueProduction' THEN
			SELECT * FROM(
				SELECT 
					 A."DocEntry" AS "DocEntry"
					,A."DocNum" AS "DocNum"
					,TO_VARCHAR(A."DueDate",'DD-MM-yyyy') AS "DueDate"
					,A."ItemCode" AS "ProductNo"
					,C."ItemName" AS "ProductName"
					,A."PlannedQty"-A."CmpltQty" AS "Qty"
					,D."Price" AS "Price"
					,CASE WHEN C."ManBtchNum"='Y' THEN
						'B'
					 WHEN C."ManSerNum"='Y' THEN
						'S'
					 ELSE
						'N'
					 END AS "ItemType"
					,C."CodeBars"
					,TO_VARCHAR(A."StartDate",'dd-MM-yyyy') AS "StartDate"
					,E."BPLid" AS "BranchCode"
					,F."BPLName" AS "BranchName"
					,A."Warehouse"
					,A."Uom"
				FROM TRIWALL_TRAINKEY."OWOR" AS A
				LEFT JOIN TRIWALL_TRAINKEY."OITM" AS C ON C."ItemCode"=A."ItemCode"
				LEFT JOIN TRIWALL_TRAINKEY."ITM1" AS D ON A."ItemCode"=D."ItemCode" AND "PriceList"=1
				LEFT JOIN TRIWALL_TRAINKEY."OWHS" AS E ON E."WhsCode"=A."Warehouse"
				LEFT JOIN TRIWALL_TRAINKEY."OBPL" AS F ON E."BPLid"=F."BPLId"
				WHERE --A."Series"=CASE WHEN :par1='-1' THEN A."Series" ELSE :par1 END 
					  --AND 
					  A."Status"='R'
					  AND A."DocEntry" NOT IN (SELECT 
					  								"BaseEntry" 
					  							FROM TRIWALL_TRAINKEY."IGN1" 
					  							WHERE "BaseType"=202)
					  AND (SELECT 
					  			COUNT(*) 	
					  	   FROM TRIWALL_TRAINKEY."WOR1" AS T0 
					  	   WHERE "DocEntry"=A."DocEntry" AND "IssuedQty"<"BaseQty")!=0
					  --AND A."DocEntry" LIKE '%'|| :par1 ||'%'
			)A 
			WHERE A."Qty"<>0
			ORDER BY "DocNum" DESC;
		ELSE IF :par1='GetForReceiptProduction' THEN
			SELECT * FROM(
				SELECT 
					 A."DocEntry" AS "DocEntry"
					,A."DocNum" AS "DocNum"
					,TO_VARCHAR(A."DueDate",'DD-MM-yyyy') AS "DueDate"
					,A."ItemCode" AS "ProductNo"
					,C."ItemName" AS "ProductName"
					,A."PlannedQty"-A."CmpltQty" AS "Qty"
					,D."Price" AS "Price"
					,CASE WHEN C."ManBtchNum"='Y' THEN
						'B'
					 WHEN C."ManSerNum"='Y' THEN
						'S'
					 ELSE
						'N'
					 END AS "ItemType"
					,C."CodeBars"
					,TO_VARCHAR(A."StartDate",'dd-MM-yyyy') AS "StartDate"
					,E."BPLid" AS "BranchCode"
					,F."BPLName" AS "BranchName"
					,A."Warehouse"
					,A."Uom"
					,G."QtyIssue"
				FROM TRIWALL_TRAINKEY."OWOR" AS A
				LEFT JOIN TRIWALL_TRAINKEY."OITM" AS C ON C."ItemCode"=A."ItemCode"
				LEFT JOIN TRIWALL_TRAINKEY."ITM1" AS D ON A."ItemCode"=D."ItemCode" AND "PriceList"=1
				LEFT JOIN TRIWALL_TRAINKEY."OWHS" AS E ON E."WhsCode"=A."Warehouse"
				LEFT JOIN TRIWALL_TRAINKEY."OBPL" AS F ON E."BPLid"=F."BPLId"
				LEFT JOIN (
					SELECT 
						 "DocEntry" AS "DocEntry"
						,SUM("IssuedQty") AS "QtyIssue"
					FROM TRIWALL_TRAINKEY."WOR1"
					GROUP BY "DocEntry"
				) AS G ON G."DocEntry"=A."DocEntry"
				WHERE --A."Series"=CASE WHEN :par1='-1' THEN A."Series" ELSE :par1 END 
					  --AND 
					  A."Status"='R'
					  AND A."DocEntry" NOT IN (SELECT 
					  								"BaseEntry" 
					  							FROM TRIWALL_TRAINKEY."IGN1" 
					  							WHERE "BaseType"=202)
					  AND G."QtyIssue"<>0
					  --AND "DocNum" LIKE '%'|| :par2 ||'%'
			)A 
			WHERE A."Qty"<>0
			ORDER BY "DocNum" DESC;
		ELSE IF :par1='GetForProductionProcess' THEN
			SELECT * FROM(
				SELECT 
					 A."DocEntry" AS "DocEntry"
					,A."DocNum" AS "DocNum"
					,TO_VARCHAR(A."DueDate",'DD-MM-yyyy') AS "DueDate"
					,A."ItemCode" AS "ProductNo"
					,C."ItemName" AS "ProductName"
					,A."PlannedQty"-A."CmpltQty" AS "Qty"
					,D."Price" AS "Price"
					,CASE WHEN C."ManBtchNum"='Y' THEN
						'B'
					 WHEN C."ManSerNum"='Y' THEN
						'S'
					 ELSE
						'N'
					 END AS "ItemType"
					,C."CodeBars"
					,TO_VARCHAR(A."StartDate",'dd-MM-yyyy') AS "StartDate"
					,E."BPLid" AS "BranchCode"
					,F."BPLName" AS "BranchName"
					,A."Warehouse"
					,A."Uom"
					,G."QtyIssue"
				FROM TRIWALL_TRAINKEY."OWOR" AS A
				LEFT JOIN TRIWALL_TRAINKEY."OITM" AS C ON C."ItemCode"=A."ItemCode"
				LEFT JOIN TRIWALL_TRAINKEY."ITM1" AS D ON A."ItemCode"=D."ItemCode" AND "PriceList"=1
				LEFT JOIN TRIWALL_TRAINKEY."OWHS" AS E ON E."WhsCode"=A."Warehouse"
				LEFT JOIN TRIWALL_TRAINKEY."OBPL" AS F ON E."BPLid"=F."BPLId"
				LEFT JOIN (
					SELECT 
						 "DocEntry" AS "DocEntry"
						,SUM("IssuedQty") AS "QtyIssue"
					FROM TRIWALL_TRAINKEY."WOR1"
					GROUP BY "DocEntry"
				) AS G ON G."DocEntry"=A."DocEntry"
				WHERE --A."Series"=CASE WHEN :par1='-1' THEN A."Series" ELSE :par1 END 
					  --AND 
					  A."Status"='R'
					  --AND "DocNum" LIKE '%'|| :par2 ||'%'
			)A 
			WHERE A."Qty"<>0
			ORDER BY "DocNum" DESC;
		ELSE IF :par1='GetProductionForFinishGoods' THEN
			SELECT * FROM(
				SELECT 
					 A."DocEntry" AS "DocEntry"
					,A."DocNum" AS "DocNum"
					,TO_VARCHAR(A."DueDate",'DD-MM-yyyy') AS "DueDate"
					,A."ItemCode" AS "ProductNo"
					,C."ItemName" AS "ProductName"
					,A."PlannedQty"-A."CmpltQty" AS "Qty"
					,D."Price" AS "Price"
					,CASE WHEN C."ManBtchNum"='Y' THEN
						'B'
					 WHEN C."ManSerNum"='Y' THEN
						'S'
					 ELSE
						'N'
					 END AS "ItemType"
					,C."CodeBars"
					,TO_VARCHAR(A."StartDate",'dd-MM-yyyy') AS "StartDate"
					,E."BPLid" AS "BranchCode"
					,F."BPLName" AS "BranchName"
					,A."Warehouse"
					,A."Uom"
					,G."QtyIssue"
				FROM TRIWALL_TRAINKEY."OWOR" AS A
				LEFT JOIN TRIWALL_TRAINKEY."OITM" AS C ON C."ItemCode"=A."ItemCode"
				LEFT JOIN TRIWALL_TRAINKEY."ITM1" AS D ON A."ItemCode"=D."ItemCode" AND "PriceList"=1
				LEFT JOIN TRIWALL_TRAINKEY."OWHS" AS E ON E."WhsCode"=A."Warehouse"
				LEFT JOIN TRIWALL_TRAINKEY."OBPL" AS F ON E."BPLid"=F."BPLId"
				LEFT JOIN (
					SELECT 
						 "DocEntry" AS "DocEntry"
						,SUM("IssuedQty") AS "QtyIssue"
					FROM TRIWALL_TRAINKEY."WOR1"
					GROUP BY "DocEntry"
				) AS G ON G."DocEntry"=A."DocEntry"
				WHERE --A."Series"=CASE WHEN :par1='-1' THEN A."Series" ELSE :par1 END 
					  --AND 
					  A."Status"='R'
					  --AND "DocNum" LIKE '%'|| :par2 ||'%'
			)A 
			WHERE A."Qty"<>0
			ORDER BY "DocNum" DESC;
		END IF;
		END IF;
		END IF;
		END IF;
	ELSE IF :DTYPE='GET_Production_Order_Count' THEN
		SELECT 
			COUNT(A."DocEntry") AS "COUNT" 
		FROM(
			SELECT 
				 A."DocEntry" AS "DocEntry"
				,A."PlannedQty"-A."CmpltQty" AS "Qty"
			FROM TRIWALL_TRAINKEY."OWOR" AS A
			LEFT JOIN TRIWALL_TRAINKEY."OITM" AS C ON C."ItemCode"=A."ItemCode"
			LEFT JOIN TRIWALL_TRAINKEY."ITM1" AS D ON A."ItemCode"=D."ItemCode" AND "PriceList"=1
			LEFT JOIN TRIWALL_TRAINKEY."OWHS" AS E ON E."WhsCode"=A."Warehouse"
			LEFT JOIN TRIWALL_TRAINKEY."OBPL" AS F ON E."BPLid"=F."BPLId"
			WHERE 
				A."Series"=CASE WHEN :par1='-1' THEN A."Series" ELSE :par1 END 
				AND A."Status"='R'
				AND A."DocEntry" NOT IN (SELECT 
											"BaseEntry" 
										 FROM TRIWALL_TRAINKEY."IGN1" 
										 WHERE "BaseType"=202)
				AND "DocNum" LIKE '%'|| :par2 ||'%'
		)A WHERE A."Qty"<>0;
	ELSE IF :DTYPE='GET_Production_Order_Web' THEN
		SELECT * FROM
		(
			SELECT 
				 A."DocEntry" AS "DocEntry"
				,A."DocNum" AS "DocNum"
				,TO_VARCHAR(A."DueDate",'dd-MMM-yyyy') AS DueDate
				,A."ItemCode" AS "ProductNo"
				,C."ItemName" AS "ProductName"
				,A."PlannedQty"-A."CmpltQty" AS "Qty"
				,D."Price" AS "Price"
				,A."Status" AS "ItemType"
				,C."CodeBars"
				,TO_VARCHAR(A."StartDate",'dd-MMM-yyyy') AS StartDate
				,E."BPLid" AS "BranchCode"
				,F."BPLName" AS "BranchName"
				,A."Warehouse"
				,A."Uom"
			FROM TRIWALL_TRAINKEY."OWOR" AS A
			LEFT JOIN TRIWALL_TRAINKEY."OITM" AS C ON C."ItemCode"=A."ItemCode"
			LEFT JOIN TRIWALL_TRAINKEY."ITM1" AS D ON A."ItemCode"=D."ItemCode" AND "PriceList"=1
			LEFT JOIN TRIWALL_TRAINKEY."OWHS" AS E ON E."WhsCode"=A."Warehouse"
			LEFT JOIN TRIWALL_TRAINKEY."OBPL" AS F ON E."BPLid"=F."BPLId"
			WHERE 
				A."Series"=CASE WHEN :par1='-1' THEN A."Series" ELSE :par1 END 
				AND A."DocEntry" NOT IN (SELECT "BaseEntry" FROM TRIWALL_TRAINKEY."IGN1" WHERE "BaseType"=202)
				AND A."Status"='P'
				--AND A."U_WebID" IS NOT NULL
				AND "DocNum" LIKE '%'|| :par2 ||'%'
		) A WHERE A."Qty"<>0;
	ELSE IF :DTYPE='SerialStatus' THEN
		SELECT * FROM(
			SELECT 
				 T0."ItemCode"
				,T5."DistNumber" AS "SerialBatch"
				,CAST(T3."OnHandQty" AS INT) AS Qty
				,T1."WhsCode"
				,T1."BinCode"
				,TO_VARCHAR(T5."ExpDate",'dd-MMM-yyyy') AS "ExpDate"
				,T5."SysNumber"
				,T4."LotNumber"
				,TO_VARCHAR(T5."MnfDate",'dd-MMM-yyyy') AS "MnfDate"
				,TO_VARCHAR(T5."InDate",'dd-MMM-yyyy') AS "AdmissionDate"
				,T5."MnfSerial" As "MfrNo"
				,'Available' As "SerialStatus"
			FROM TRIWALL_TRAINKEY."OIBQ" T0
			INNER JOIN TRIWALL_TRAINKEY."OBIN" T1 ON T0."BinAbs"=T1."AbsEntry" AND T0."OnHandQty" <> 0
			LEFT OUTER JOIN TRIWALL_TRAINKEY."OBBQ" T2 ON T2."BinAbs"=T0."BinAbs" AND T2."ItemCode"=T0."ItemCode" AND T2."OnHandQty" <> 0
			LEFT OUTER JOIN TRIWALL_TRAINKEY."OSBQ" T3 ON T3."BinAbs"=T0."BinAbs" AND T3."ItemCode"=T0."ItemCode" AND T3."OnHandQty" <> 0
			LEFT OUTER JOIN TRIWALL_TRAINKEY."OBTN" T4 ON T2."SnBMDAbs"=T4."AbsEntry" AND T2."ItemCode"=T4."ItemCode"
			LEFT OUTER JOIN TRIWALL_TRAINKEY."OSRN" T5 ON T3."SnBMDAbs"=T5."AbsEntry" AND T3."ItemCode"=T5."ItemCode"
			WHERE
				T1."AbsEntry" >= 0
				AND (T3."AbsEntry" IS NOT NULL)
				AND T0."ItemCode" IN (SELECT 
										U0."ItemCode" 
									  FROM TRIWALL_TRAINKEY."OITM" U0 
									  INNER JOIN TRIWALL_TRAINKEY."OITB" U1 on U0."ItmsGrpCod" = U1."ItmsGrpCod"
									  WHERE U0."ItemCode" IS NOT NULL 
									 )
				AND T0."ItemCode" IN (SELECT * FROM LIBRARY:SPLIT_TO_TABLE(:par2,','))
			UNION ALL
			SELECT 
				 A."ItemCode"
				,B."DistNumber"
				,A."Quantity" As Qty
				,A."WhsCode"
				,'' As "BinCode"
				,B."ExpDate"
				,A."SysNumber"
				,B."LotNumber"
				,B."MnfDate"
				,B."InDate" As "AdmissionDate"
				,B."MnfSerial"
				,'UnAvailable' As "SerialStatus"
			FROM TRIWALL_TRAINKEY."OSRQ" A 
			LEFT JOIN TRIWALL_TRAINKEY."OSRN" B ON A."ItemCode"=B."ItemCode" AND A."SysNumber"=B."SysNumber" AND A."MdAbsEntry"=B."AbsEntry"
			LEFT JOIN TRIWALL_TRAINKEY."OWHS" C ON C."WhsCode"=A."WhsCode" 
			WHERE 
				A."Quantity"<>0 
				AND A."ItemCode" IN (SELECT * FROM LIBRARY:SPLIT_TO_TABLE(:par2,','))
				AND C."BinActivat"='N'
		)A 
		WHERE 
			"WhsCode"=:par1 
			AND "ItemCode"=:par2;
	ELSE IF :DTYPE='Print_Barcode' THEN
		SELECT TOP 10 
			 "ItemCode"
			,'100.38' AS "Price"
			,'*'|| "CodeBars" ||'*' AS "CodeBars" 
		FROM TRIWALL_TRAINKEY."OITM" 
		WHERE IFNULL("CodeBars",'')<>'';
	ELSE IF :DTYPE='GETLayouts' THEN
		/*SELECT 
			 Code
			,Name
			,U_Addess As [Address]
		FROM [@TBKOFIWEBPRINTING]
		WHERE U_LayoutModule=@par1*/
	ELSE IF :DTYPE='USP_KOFI_BARCODE_PRINTING' THEN
		SELECT 
			 ROW_NUMBER() OVER(ORDER BY "DocNum") AS "No"
			,T0."DocEntry"
			,T5."SeriesName"
			,T0."CardCode"
			,T0."CardName"
			,T0."DocNum"
			,T0."DocTotal"
			,T0."DocDate"
			,T0."DocDueDate"
			,T0."NumAtCard"
			,T0."Comments"
			,T0."BPLName" As "Branch"
			,T1."ItemCode"
			,T1."Dscription"
			,Cast(T1."Quantity" As INT) AS "Quantity"
			,TO_DECIMAL(T1."Price",12,2) As "Price"
			,TO_DECIMAL(T1."LineTotal",9,2) As "LineTotal"
			,T1."BaseEntry"
			,T1."BaseLine"
			,T1."UomCode"
			,T1."WhsCode"
			,CASE WHEN T4."ManBtchNum"='Y' THEN 
				'B' 
			 ELSE 
			 	CASE WHEN T4."ManSerNum"='Y' THEN 
			 		'S' 
			 	ELSE 
			 		'N' 
			 	END 
			 END AS "ItemType"
			,T4."CodeBars"
			,TO_DECIMAL(T0."DocTotal",12,2) As "DocTotal"
			,''/*U_docDraft*/ As "PONumber"
			,T0."BPLName"
		FROM TRIWALL_TRAINKEY."ODRF" T0
		INNER JOIN TRIWALL_TRAINKEY."DRF1" T1 ON T0."DocEntry"=T1."DocEntry"
		LEFT JOIN TRIWALL_TRAINKEY."DRF16" T2 ON T1."DocEntry"=T2."AbsEntry" AND T1."LineNum"=T2."LineNum"
		LEFT JOIN TRIWALL_TRAINKEY."ODBN" T3 ON T2."ObjId"=T3."ObjType" AND T2."ObjAbs"=T3."AbsEntry"
		LEFT JOIN TRIWALL_TRAINKEY."OITM" T4 ON T4."ItemCode"=T1."ItemCode"
		LEFT JOIN TRIWALL_TRAINKEY."NNM1" T5 ON T5."Series"=T0."Series" And T5."ObjectCode"=20
		WHERE T0."ObjType"=20  And T0."DocEntry"=:par1;
	ELSE IF :DTYPE='Delivery_Print' THEN
		SELECT  
			 D0."CardCode"
			,D0."Address" AS "ShipTO"
			,D0."Address2" AS "BillTo"
			,D0."DocNum"
			,D0."DocDate"
			,''/*D0."U_DeliCon"*/ AS DeliveryCOndition
			,D1."ItemCode"
			,D1."Dscription" AS Description
			,D1."Quantity" As Qty
		FROM TRIWALL_TRAINKEY."ODLN" AS D0 
		LEFT JOIN TRIWALL_TRAINKEY."DLN1" D1 ON D0."DocEntry"=D1."DocEntry"
		WHERE D0."DocEntry"=:par1;
	ELSE IF :DTYPE='Return_Delivery_Print' THEN
		SELECT 
			 ROW_NUMBER() OVER(ORDER BY "DocNum") As "No"
			,T0."DocEntry"
			,T5."SeriesName"
			,T0."CardCode"
			,T0."CardName"
			,T0."DocNum"
			,T0."DocTotal"
			,TO_VARCHAR(T0."DocDate",'dd-MMM-yyyy') As "DocDate"
			,T0."DocDueDate"
			,T0."NumAtCard"
			,T0."Comments"
			,T0."BPLName" As "Branch"
			,T1."ItemCode"
			,T1."Dscription" As "Description"
			,Cast(T1."Quantity" As INT) AS "Qty"
			,Cast(T1."Price" As Numeric(9,2)) As "Price"
			,Cast(T1."LineTotal" As Numeric(9,2)) As "LineTotal"
			,T1."DiscPrcnt"
			,T1."BaseEntry"
			,T1."BaseLine"
			,T1."UomCode"
			,T6."Phone1"
			,T6."Phone2"
			,T0."Address2" As "BillTo"
			,T0."Address" As "ShipTo"
			,T1."WhsCode"
			,CASE WHEN T4."ManBtchNum"='Y' THEN 
				'B' 
			 ELSE 
			 	CASE WHEN T4."ManSerNum"='Y' THEN 
			 		'S' 
			 	ELSE 
			 		'N' 
			 	END 
			 END As "ItemType"
			,T4."CodeBars"
			,Cast(T0."DocTotal" As Numeric(9,2)) As "DocTotal"
			,''/*"U_docDraft"*/ As "PONumber"
			,T0."BPLName"
			,C."ItmsGrpNam" As "Brand"
			,''/*T0."U_docDraft"*/ As "BaseNum"
			,'' As "SerialBatch"
			,T1."BaseLine" As "BaseLinNum"
			,''/*T0."U_DeliCon"*/ As "DeliveryCondition"
		FROM TRIWALL_TRAINKEY."ODRF" T0
		INNER JOIN TRIWALL_TRAINKEY."DRF1" T1 ON T0."DocEntry"=T1."DocEntry"
		LEFT JOIN TRIWALL_TRAINKEY."DRF16" T2 ON T1."DocEntry"=T2."AbsEntry" AND T1."LineNum"=T2."LineNum"
		LEFT JOIN TRIWALL_TRAINKEY."ODBN" T3 ON T2."ObjId"=T3."ObjType" AND T2."ObjAbs"=T3."AbsEntry"
		LEFT JOIN TRIWALL_TRAINKEY."OITM" T4 ON T4."ItemCode"=T1."ItemCode"
		LEFT JOIN TRIWALL_TRAINKEY."NNM1" T5 ON T5."Series"=T0."Series" And T5."ObjectCode"=16
		LEFT JOIN TRIWALL_TRAINKEY."OCRD" T6 ON T6."CardCode"=T0."CardCode"
		LEFT JOIN TRIWALL_TRAINKEY."KPPT" B On T1."ItemCode"=B."ItemCode"
		LEFT JOIN TRIWALL_TRAINKEY."OITG" C ON C."ItmsTypCod"=B."ItmsTypCod"
		WHERE T0."ObjType"=16 and T1."DocEntry"=:par1;
	ELSE IF :DTYPE='Credit_Memo_Print' THEN
		SELECT 
			 ROW_NUMBER() OVER(ORDER BY "DocNum") As "No"
			,T0."DocEntry"
			,T5."SeriesName"
			,T0."CardCode"
			,T0."CardName"
			,T0."DocNum"
			,T0."DocTotal"
			,T0."DocDate"
			,T0."DocDueDate"
			,T0."NumAtCard"
			,T0."Comments"
			,T0."BPLName" As "Branch"
			,T1."ItemCode"
			,T1."Dscription"
			,Cast(T1."Quantity" As INT) AS "Quantity"
			,TO_DECIMAL(T1."Price",9,2) As "Price"
			,TO_DECIMAL(T1."LineTotal",9,2) As "LineTotal"
			,T1."DiscPrcnt"
			,T1."BaseEntry"
			,T1."BaseLine"
			,T1."UomCode"
			,T6."Phone1"
			,T6."Phone2"
			,T0."Address2"
			,T0."Address"
			,T1."WhsCode"
			,CASE WHEN T4."ManBtchNum"='Y' THEN 
				'B'
			 ELSE 
			 	CASE WHEN T4."ManSerNum"='Y' THEN 
			 		'S' 
			 	ELSE 
			 		'N'
			 	END 
			 END As "ItemType"
			,T4."CodeBars"
			,TO_DECIMAL(T0."DocTotal",12,2) As "DocTotal"
			,''/*U_docDraft*/ As "PONumber"
			,T0."BPLName"
		FROM TRIWALL_TRAINKEY."ODRF" T0
		INNER JOIN TRIWALL_TRAINKEY."DRF1" T1 ON T0."DocEntry"=T1."DocEntry"
		LEFT JOIN TRIWALL_TRAINKEY."DRF16" T2 ON T1."DocEntry"=T2."AbsEntry" AND T1."LineNum"=T2."LineNum"
		LEFT JOIN TRIWALL_TRAINKEY."ODBN" T3 ON T2."ObjId"=T3."ObjType" AND T2."ObjAbs"=T3."AbsEntry"
		LEFT JOIN TRIWALL_TRAINKEY."OITM" T4 ON T4."ItemCode"=T1."ItemCode"
		LEFT JOIN TRIWALL_TRAINKEY."NNM1" T5 ON T5."Series"=T0."Series" And T5."ObjectCode"=14
		LEFT JOIN TRIWALL_TRAINKEY."OCRD" T6 ON T6."CardCode"=T0."CardCode"
		WHERE T0."ObjType"=14 and T1."DocEntry"=:par1;
	ELSE IF :DTYPE='Goods_Return' THEN
		SELECT 
			ROW_NUMBER() Over(Order by "BaseLinNum") AS "No"
			,C."ItmsGrpNam" AS "Brand"
			,A.* 
		FROM(
			SELECT	
				 A."CardCode"
				,A."CardName"
				,A."DocDate"
				,A."BaseNum"
				,A."ItemCode" AS "ItemCode"	
				,B."DistNumber" AS "SerialBatch"
				,1 AS "Qty"
				,A."BaseLinNum"
			FROM TRIWALL_TRAINKEY."SRI1" AS A
			LEFT JOIN TRIWALL_TRAINKEY."OSRN" AS B ON A."ItemCode"=B."ItemCode" AND B."SysNumber"=A."SysSerial"
			LEFT JOIN TRIWALL_TRAINKEY."RPD1" C ON C."DocEntry"=A."BaseEntry" And C."VisOrder"=A."BaseLinNum"
			LEFT JOIN TRIWALL_TRAINKEY."ORPD" D ON D."DocEntry"=C."DocEntry"
			WHERE A."BaseType"=21 and d."DocEntry"=19
			UNION All
			SELECT 
				 G0."CardCode"
				,G0."CardName"
				,G0."DocDate"
				,G1."BaseDocNum" AS "BaseNum"
				, G1."ItemCode"
				,'' AS "SerialBacth"
				, G1."Quantity" AS "Qty"
				, G1."VisOrder" AS "BaseLinNum"
			FROM TRIWALL_TRAINKEY."ORPD" AS  G0
			LEFT JOIN TRIWALL_TRAINKEY."RPD1" G1 ON G1."DocEntry"=G0."DocEntry"
			LEFT JOIN TRIWALL_TRAINKEY."OITM" G2 ON G1."ItemCode"=G2."ItemCode"
			WHERE G0."DocEntry"=:par1 AND "ManSerNum"<>'Y'
		) AS A
		LEFT JOIN TRIWALL_TRAINKEY."KPPT" B ON A."ItemCode"=B."ItemCode"
		LEFT JOIN TRIWALL_TRAINKEY."OITG" C ON C."ItmsTypCod"=B."ItmsTypCod";
	ELSE IF :DTYPE='Draft' THEN
		IF :par1='20' THEN
			SELECT 
				 T0."DocNum"
				,T0."DocDate"
				,T0."CardCode"
				,T0."CardName"
				,T0."DocTotal"
				,T0."NumAtCard"
				,T0."BPLName" 
			FROM TRIWALL_TRAINKEY."ODRF" T0
			INNER JOIN TRIWALL_TRAINKEY."DRF1" T1 ON T0."DocEntry"=T1."DocEntry"
			LEFT JOIN TRIWALL_TRAINKEY."DRF16" T2 ON T1."DocEntry"=T2."AbsEntry" AND T1."LineNum"=T2."LineNum"
			LEFT JOIN TRIWALL_TRAINKEY."ODBN" T3 ON T2."ObjId"=T3."ObjType" AND T2."ObjAbs"=T3."AbsEntry"
			WHERE T0."ObjType"=20 And T0."DocStatus"='O';
		IF :par1='20Line' THEN
			SELECT 
				 T5."SeriesName"
				,T0."CardCode"
				,T0."CardName"
				,T0."DocNum"
				,T0."DocTotal"
				,T0."DocDate"
				,T0."DocDueDate"
				,T0."NumAtCard"
				,T0."Comments"
				,T0."BPLName" As "Branch"
				,T1."ItemCode"
				,T1."Dscription"
				,T1."Quantity"
				,T1."Price"
				,T1."LineTotal"
				,T1."BaseEntry"
				,T1."BaseLine"
				,T1."UomCode"
				,T1."WhsCode"
				,CASE WHEN T4."ManBtchNum"='Y' THEN 
					'B'
				 ELSE 
				 	CASE WHEN T4."ManSerNum"='Y' THEN 
				 		'S'
				 	ELSE 
				 		'N'
				 	END 
				 END As "ItemType"
				,T4."CodeBars"
			FROM TRIWALL_TRAINKEY."ODRF" T0
			INNER JOIN TRIWALL_TRAINKEY."DRF1" T1 ON T0."DocEntry"=T1."DocEntry"
			LEFT JOIN TRIWALL_TRAINKEY."DRF16" T2 ON T1."DocEntry"=T2."AbsEntry" AND T1."LineNum"=T2."LineNum"
			LEFT JOIN TRIWALL_TRAINKEY."ODBN" T3 ON T2."ObjId"=T3."ObjType" AND T2."ObjAbs"=T3."AbsEntry"
			LEFT JOIN TRIWALL_TRAINKEY."OITM" T4 ON T4."ItemCode"=T1."ItemCode"
			LEFT JOIN TRIWALL_TRAINKEY."NNM1" T5 ON T5."Series"=T0."Series" AND T5."ObjectCode"=20
			WHERE T0."ObjType"=20 AND T0."DocStatus"='O'  AND T0."DocNum"=:par2;
		END IF;
		END IF;
	ELSE IF :DTYPE='ARDocumentHeader' THEN
		SELECT  
			 "DocNum"
			,"DocDate"
			,"CardCode"
			,"CardName"
			,"NumAtCard"
			,"BPLName"
			,"DocTotal"
			,"DocDraft" 
		FROM (
			SELECT DISTINCT
			 A."DocNum" AS "DocNum"
			,TO_VARCHAR(A."DocDate",'dd/MM/yyyy') As "DocDate"
			,A."CardCode"
			,A."CardName"
			,A."NumAtCard"
			,A."BPLName"
			,A."DocTotal"
			,''/*A."U_docDraft"*/ AS "DocDraft"
			,A."DocEntry"
			FROM TRIWALL_TRAINKEY."OINV" A 
			LEFT JOIN TRIWALL_TRAINKEY."NNM1" B ON A."Series"=B."Series" AND B."ObjectCode"=13
			LEFT JOIN TRIWALL_TRAINKEY."INV1" C ON A."DocEntry"=C."DocEntry"
			LEFT JOIN TRIWALL_TRAINKEY."OITM" D ON D."ItemCode"=C."ItemCode"
			WHERE 
				A."Series"=CASE WHEN :par1='' THEN A."Series"  WHEN :par1='All' THEN A."Series" ELSE :par1 END 
				AND "DocNum" LIKE '%' || :par2 || '%'
				AND C."LineStatus"='O' 
				AND A."DocStatus"='O' 
				AND A."DocType"='I' 
				--AND IFNULL(A."U_docDraft",'')<>'oDraft'
		)A ORDER BY "DocEntry" DESC;
		/*OFFSET cast(@par3 as int) ROWS
		FETCH NEXT cast(@par4 as int) ROWS ONLY;*/
	ELSE IF :DTYPE='ARDocumentHeaderCount' THEN
		SELECT 
			COUNT(A."DocNum") AS "Count" 
		FROM (
			SELECT DISTINCT 
				"DocNum" AS "DocNum"
			FROM TRIWALL_TRAINKEY."OINV" A 
			LEFT JOIN TRIWALL_TRAINKEY."NNM1" B ON A."Series"=B."Series" And B."ObjectCode"=13
			LEFT JOIN TRIWALL_TRAINKEY."INV1" C ON A."DocEntry"=C."DocEntry"
			LEFT JOIN TRIWALL_TRAINKEY."OITM" D ON D."ItemCode"=C."ItemCode"	 
			WHERE 
				A."Series"=CASE WHEN :par1='' THEN A."Series"  WHEN :par1='All' THEN A."Series" ELSE :par1 END 
				AND "DocNum" LIKE '%' || :par2 || '%'
				AND C."LineStatus"='O' 
				AND A."DocStatus"='O' 
				AND A."DocType"='I'
				/*AND IFNULL(A."U_docDraft",'')<>'oDraft';*/
		) A;
	ELSE IF :DTYPE='ARDocumentLine' THEN
		SELECT 
			 B."DocNum"
			,A."LineNum"
			,B."DocDate"
			,B."DocDueDate"
			,B."CardCode"
			,B."CardName"
			,IFNULL(B."NumAtCard",'') As "NumAtCard"
			,C."SeriesName"
			,B."BPLId"
			,B."BPLName"
			,B."DocTotal"
			,A."ItemCode"
			,A."Dscription"
			,A."OpenQty" As "Quantity"
			,A."OpenQty"
			,A."Price"
			,A."LineTotal"
			,A."UomCode"
			,A."VatGroup"
			,A."WhsCode"
			,A."LineNum"
			,A."DocEntry"
			,D."CodeBars"
			,CASE WHEN D."ManBtchNum"='Y' THEN 
				'B'
			 ELSE 
			 	CASE WHEN D."ManSerNum"='Y' THEN 
			 		'S' 
			 	ELSE 
			 		'N' 
			 	END 
			 END As "ManItem"
			,IFNULL(B."Comments",'') As "Comments"
		FROM TRIWALL_TRAINKEY."INV1" A 
		LEFT JOIN TRIWALL_TRAINKEY."OINV" B ON A."DocEntry"=B."DocEntry"
		LEFT JOIN TRIWALL_TRAINKEY."NNM1" C ON C."Series"=B."Series" And C."ObjectCode"=13
		LEFT JOIN TRIWALL_TRAINKEY."OITM" D ON D."ItemCode"=A."ItemCode"
		WHERE 
			B."DocType"='I' 
			AND B."DocStatus"='O' 
			AND A."LineStatus"='O'
			And B."DocNum"=:par1;
	ELSE IF :DTYPE='GetARSerial' THEN
		Declare U_Serial NVARCHAR(5000);
		Declare AREntry INT;
		CREATE LOCAL TEMPORARY TABLE #TMPAR_Memo(
			 "ItemCode" NVARCHAR(5000)
			,"Serial" NVARCHAR(5000)
			,"Qty" INT
			,"LotNumber" NVARCHAR(5000)
			,"ExpDate" DATE
			,"MnfSerial" NVARCHAR(5000)
			,"MnfDate" DATE
			,"SysSerial" NVARCHAR(5000)
			,"AdmissionDate" DATE
			,"OnHandQty" INT
		);
		SELECT "DocEntry" INTO AREntry FROM TRIWALL_TRAINKEY."OINV" WHERE "DocNum"=:par1;
		SELECT 
			/*IFNULL("U_OutSerial",'')*/'' INTO U_Serial 
		FROM TRIWALL_TRAINKEY."INV1" 
		WHERE "DocEntry"=:AREntry And "ItemCode"=:par2;
		SELECT 
			 B."ItemCode"
			,CASE WHEN B."BaseType"=15 THEN C."Serial" ELSE D."Serial" END AS "Serial"
			,1 AS "Qty"
			,CASE WHEN B."BaseType"=15 THEN C."LotNumber" ELSE D."LotNumber" END AS "LotNumber"
			,CASE WHEN B."BaseType"=15 THEN C."ExpDate" ELSE D."ExpDate" END AS "ExpDate"
			,CASE WHEN B."BaseType"=15 THEN C."MnfSerial" ELSE D."MnfSerial" END AS "MnfSerial"
			,CASE WHEN B."BaseType"=15 THEN C."MnfDate" ELSE D."MnfDate" END As "MnfDate"
			,CASE WHEN B."BaseType"=15 THEN C."SysSerial" ELSE D."SysNumber" END AS "SysSerial"
			,CASE WHEN B."BaseType"=15 THEN C."InDate" ELSE D."InDate" END AS "AdmissionDate"
			,CASE WHEN IFNULL(C."OnHandQty",0)>0 THEN C."OnHandQty" ELSE IFNULL(D."OnHandQty",0) END AS "OnHandQty"
		FROM TRIWALL_TRAINKEY."OINV" A
		LEFT JOIN TRIWALL_TRAINKEY."INV1" B ON A."DocEntry"=B."DocEntry"
		LEFT JOIN TRIWALL_TRAINKEY."DLN1" T0 ON T0."DocEntry"=B."BaseEntry" And T0."LineNum"=B."BaseLine"
		LEFT JOIN TRIWALL_TRAINKEY."ODLN" T1 ON T0."DocEntry"=T1."DocEntry"
		LEFT JOIN TRIWALL_TRAINKEY."OITM" I ON I."ItemCode"=B."ItemCode"
		LEFT JOIN (
			SELECT 
				 T0."BaseNum"
				,T0."ItemCode"
				,T0."BaseLinNum"
				,T1."DistNumber" As "Serial"
				,T1."LotNumber"
				,TO_VARCHAR("ExpDate",'dd-MM-yyyy') AS "ExpDate" 
				,1 As "Qty"
				,T1."MnfSerial"
				,T1."MnfDate"
				,T0."SysSerial"
				,TO_VARCHAR(T1."CreateDate",'dd-MM-yyyy') As "InDate"
				,T2."OnHandQty"
			FROM TRIWALL_TRAINKEY."SRI1" T0
			LEFT JOIN TRIWALL_TRAINKEY."OSRN" T1 ON T0."ItemCode"=T1."ItemCode" And T0."SysSerial"=T1."SysNumber"
			LEFT JOIN TRIWALL_TRAINKEY."OSBQ" T2 ON T2."SnBMDAbs"=T1."AbsEntry" And T2."ItemCode"=T1."ItemCode" 
			WHERE T0."BaseType"=15
		)C ON C."BaseNum"=T1."DocNum" And B."ItemCode"=C."ItemCode"
		LEFT JOIN (
			SELECT 
				 T0."BaseNum"
				,T0."ItemCode"
				,T0."BaseLinNum"
				,T1."DistNumber" As "Serial"
				,T1."LotNumber"
				,TO_VARCHAR("ExpDate",'dd-MM-yyyy') AS "ExpDate" 
				,1 As "Qty"
				,T1."MnfSerial"
				,T1."MnfDate"
				,T1."SysNumber"
				,TO_VARCHAR(T1."CreateDate",'dd-MMM-yyyy') As "InDate"
				,T2."OnHandQty"
			FROM TRIWALL_TRAINKEY."SRI1" T0
			LEFT JOIN TRIWALL_TRAINKEY."OSRN" T1 ON T0."ItemCode"=T1."ItemCode" And T0."SysSerial"=T1."SysNumber"
			LEFT JOIN TRIWALL_TRAINKEY."OSBQ" T2 ON T2."SnBMDAbs"=T1."AbsEntry" And T2."ItemCode"=T1."ItemCode"
			WHERE T0."BaseType"=13
		)D ON D."BaseNum"=A."DocNum" And B."ItemCode"=D."ItemCode"
		WHERE 
			A."CANCELED"='N' 
			AND I."ManSerNum"='Y'
			AND A."DocNum"=:par1 
			AND B."ItemCode"=:par2 
		INTO #TMPAR_Memo;
		SELECT DISTINCT 
			* 
		FROM #TMPAR_Memo 
		WHERE "OnHandQty"=0;
		DROP TABLE #TMPAR_Memo;
	ELSE IF :DTYPE='GetARBatch' THEN
		DECLARE NumberDO INT;
		SELECT TOP 1 
			T1."DocNum" INTO NumberDO
		FROM TRIWALL_TRAINKEY."OINV" A
		LEFT JOIN TRIWALL_TRAINKEY."INV1" B ON A."DocEntry"=B."DocEntry"
		LEFT JOIN TRIWALL_TRAINKEY."DLN1" T0 ON T0."DocEntry"=B."BaseEntry" AND T0."LineNum"=B."BaseLine"
		LEFT JOIN TRIWALL_TRAINKEY."ODLN" T1 ON T0."DocEntry"=T1."DocEntry"
		WHERE A."DocNum"=:par1 AND A."DocStatus"='O';
		SELECT DISTINCT * FROM (
			SELECT 
				 B."ItemCode"
				,CASE WHEN B."BaseType"=15 THEN C."Batch" ELSE D."Batch" END AS "Batch"
				,CASE WHEN B."BaseType"=15 THEN C."Qty" ELSE D."Qty" END AS "Qty"
				,CASE WHEN B."BaseType"=15 THEN C."LotNumber" ELSE D."LotNumber" END AS "LotNumber"
				,CASE WHEN B."BaseType"=15 THEN C."ExpDate" ELSE D."ExpDate" END AS "ExpDate"
				,CASE WHEN B."BaseType"=15 THEN C."MnfSerial" ELSE D."MnfSerial" END AS "MnfSerial"
				,CASE WHEN B."BaseType"=15 THEN C."MnfDate" ELSE D."MnfDate" END AS "MnfDate"
				,CASE WHEN B."BaseType"=15 THEN C."SysNumber" ELSE D."SysNumber" END AS "SysNumber"
				,CASE WHEN B."BaseType"=15 THEN C."InDate" ELSE D."InDate" END AS "AdmissionDate"
			FROM TRIWALL_TRAINKEY."OINV" A
			LEFT JOIN TRIWALL_TRAINKEY."INV1" B ON A."DocEntry"=B."DocEntry"
			LEFT JOIN TRIWALL_TRAINKEY."DLN1" T0 ON T0."DocEntry"=B."BaseEntry" And T0."LineNum"=B."BaseLine"
			LEFT JOIN TRIWALL_TRAINKEY."ODLN" T1 ON T0."DocEntry"=T1."DocEntry"
			LEFT JOIN TRIWALL_TRAINKEY."OITM" I ON I."ItemCode"=B."ItemCode"
			LEFT JOIN (
				SELECT * FROM (
					SELECT 
						 T0."BaseNum"
						,T0."ItemCode"
						,T0."BaseLinNum"
						,T1."DistNumber" AS "Batch"
						,T1."LotNumber"
						,TO_VARCHAR("ExpDate",'dd-MM-yyyy') AS "ExpDate"
						,T0."Quantity"
							-
						 IFNULL(
						 	(SELECT 
						 		SUM("Quantity") 
						 	 FROM TRIWALL_TRAINKEY."IBT1" C 
						 	 WHERE 
						 	 	C."BsDocType"=15 
						 	 	AND C."BsDocEntry"=T0."BaseEntry" 
						 	 	AND C."BsDocLine"=T0."BaseLinNum" 
						 	 	AND C."BatchNum"=T0."BatchNum"
						 	),0)
							-
						IFNULL(
							(SELECT 
								SUM(D."Quantity") 
							 FROM TRIWALL_TRAINKEY."IBT1" D 
							 WHERE 
							 	D."BaseType"=14 
							 	AND D."BatchNum"=T0."BatchNum" 
							 	AND "BaseEntry"=(SELECT DISTINCT 
							 						D1."TrgetEntry" 
							 					FROM TRIWALL_TRAINKEY."INV1" D1 
							 					WHERE 
							 						D1."BaseEntry"=T0."BaseEntry" 
							 						AND D1."BaseLine"=T0."BaseLinNum")
						 ),0) AS "Qty"
						,T1."MnfSerial"
						,T1."MnfDate"
						,T0."BatchNum"
						,TO_VARCHAR(T1."CreateDate",'dd-MM-yyyy') AS "InDate"
						,T1."SysNumber"
					FROM TRIWALL_TRAINKEY."IBT1" T0
					LEFT JOIN TRIWALL_TRAINKEY."OBTN" T1 ON T0."ItemCode"=T1."ItemCode" AND T0."BatchNum"=T1."DistNumber"
					LEFT JOIN TRIWALL_TRAINKEY."OBTQ" T2 ON T2."ItemCode"=T1."ItemCode" AND T2."SysNumber"=T1."SysNumber"
					WHERE 
						T0."BaseType" IN(15) 
						AND T0."BaseNum"=:NumberDO 
						AND T0."ItemCode"=:par2
				)A WHERE A."Qty"<>0
			)C ON C."BaseNum"=T1."DocNum" And B."ItemCode"=C."ItemCode"
			LEFT JOIN (
				SELECT 
					 T0."BaseNum"
					,T0."ItemCode"
					,T0."BaseLinNum"
					,T1."DistNumber" AS "Batch"
					,T1."LotNumber"
					,TO_VARCHAR("ExpDate",'dd-MM-yyyy') AS "ExpDate" 
					,T0."Quantity" 
						-
					 IFNULL(
					 	(SELECT 
					 		SUM("Quantity") 
					 	 FROM TRIWALL_TRAINKEY."IBT1" C 
					 	 WHERE 
					 	 	C."BaseType"=14 
					 	 	AND C."BatchNum"=T0."BatchNum" 
					 	 	AND C."BaseEntry"=(SELECT DISTINCT 
					 	 						C1."TrgetEntry" 
					 	 					 FROM TRIWALL_TRAINKEY."INV1" C1 WHERE C1."DocEntry"=T0."BaseEntry")
					 ),0) As "Qty"
					,T1."MnfSerial"
					,T1."MnfDate"
					,T1."SysNumber"
					,TO_VARCHAR(T1."CreateDate",'dd-MMM-yyyy') As "InDate"
				FROM TRIWALL_TRAINKEY."IBT1" T0
				LEFT JOIN TRIWALL_TRAINKEY."OBTN" T1 ON T0."ItemCode"=T1."ItemCode" AND T0."BatchNum"=T1."DistNumber"
				LEFT JOIN TRIWALL_TRAINKEY."OBTQ" T2 ON T2."ItemCode"=T1."ItemCode" AND T2."SysNumber"=T1."SysNumber" 
				WHERE 
					T0."BaseType"=13 
					AND T0."BaseNum"=:par1 
					AND T0."ItemCode"=:par2
			)D ON D."BaseNum"=A."DocNum" AND B."ItemCode"=D."ItemCode"
			WHERE A."CANCELED"='N' AND I."ManBtchNum"='Y' AND A."DocNum"=:par1 AND B."ItemCode"=:par2
		) A;
	ELSE IF :DTYPE='oDraft' THEN
		IF :par1='22' THEN
		--update PO that has Save Goods Receipt PO to Draft
			DECLARE _Close INT;
			DECLARE POEntry INT;
			
			SELECT 
				"DocEntry" INTO POEntry 
			FROM TRIWALL_TRAINKEY."OPOR" 
			WHERE TO_VARCHAR("DocNum")=:par2;
			
			/*UPDATE TRIWALL_TRAINKEY."POR1" 
				SET "U_oDraftStatus"=:par4 
			WHERE 
				"DocEntry"=:POEntry 
				AND "LineNum"=:par5;*/ -- Update PO Line Status

			/*UPDATE TRIWALL_TRAINKEY."POR1"
				SET "U_ODraftQty"=CAST(:par3 AS NUMERIC) 
				WHERE "DocEntry"=:POEntry AND "LineNum"=:par5;*/

			SELECT 
				COUNT("ItemCode") INTO _Close 
			FROM TRIWALL_TRAINKEY."POR1" 
			WHERE 
				"DocEntry"=:POEntry;
				/*AND IFNULL("U_oDraftStatus",'')<>'C'*/--Check status for update Document Header PO

			IF :_Close>0 THEN
				/*UPDATE TRIWALL_TRAINKEY."OPOR" 
					SET "U_docDraft"='' 
				WHERE "DocEntry"=CAST(:POEntry AS INT);*/
			END IF;
			IF :_Close=0 THEN
				/*UPDATE TRIWALL_TRAINKEY."OPOR" 
					SET "U_docDraft"='oDraft' 
				WHERE "DocEntry"=CAST(:POEntry AS INT);*/
			END IF;
			SELECT 
				"DocNum" 
			FROM TRIWALL_TRAINKEY."OPOR" 
			WHERE "DocNum"=:par2;
		
		IF :par1='15' THEN
		
			DECLARE Close16 INT;
			DECLARE POEntry16 INT;
			DECLARE Qty1 INT;
			SELECT 
				"DocEntry" INTO POEntry16 
			FROM TRIWALL_TRAINKEY."ODLN" 
			WHERE TO_VARCHAR("DocNum")=:par2;

			SELECT 
				/*IFNULL("U_ODraftQty",0)*/0 INTO Qty1 
			FROM TRIWALL_TRAINKEY."DLN1" 
			WHERE 
				"DocEntry"=:POEntry16 
				And "VisOrder"=:par5;

			/*UPDATE TRIWALL_TRAINKEY."DLN1" 
				SET "U_oDraftStatus"=:par4 
			WHERE 
				"DocEntry"=:POEntry16 
				And "VisOrder"=:par5;*/ -- Update PO Line Status
			/*UPDATE TRIWALL_TRAINKEY."DLN1"
				SET "U_ODraftQty"=(CAST(:par3 AS NUMERIC)+CAST(:Qty1 AS NUMERIC))
			WHERE 
				"DocEntry"=:POEntry16 
				AND "VisOrder"=:par5;*/

			SELECT 
				COUNT("ItemCode") INTO Close16 
			FROM TRIWALL_TRAINKEY."DLN1" 
			Where 
				"DocEntry"=:POEntry16;
				/*AND IFNULL("U_oDraftStatus",'')<>'C'*/ --Check status for update Document Header PO
			IF :Close16>0 THEN
				/*UPDATE TRIWALL_TRAINKEY."ODLN" 
					SET "U_docDraft"='' 
				WHERE TO_VARCHAR("DocEntry")=:POEntry16;*/
			END IF;
			
			IF :Close16=0 THEN
				/*UPDATE TRIWALL_TRAINKEY."ODLN" 
					SET "U_docDraft"='oDraft' 
				WHERE TO_VARCHAR("DocEntry")=:POEntry16;*/
			END IF;
			
			SELECT 
				"DocNum" 
			FROM TRIWALL_TRAINKEY."ODLN" 
			WHERE "DocNum"=:par2;
			
		IF :par1='13' THEN
		--AR Invoice Save AR Memo Draft
			Declare Close13 INT;
			Declare POEntry13 INT;
			Declare Qty13 INT;
			SELECT 
				"DocEntry" INTO POEntry13 
			FROM TRIWALL_TRAINKEY."OINV" 
			WHERE TO_VARCHAR("DocNum")=:par2;

			SELECT 
				/*IFNULL("U_ODraftQty",0)*/ 0 INTO Qty13 
			FROM TRIWALL_TRAINKEY."INV1" 
			WHERE 
				"DocEntry"=:POEntry13 
				AND "VisOrder"=:par5;
				
			/*UPDATE TRIWALL_TRAINKEY."INV1" 
				SET "U_oDraftStatus"=:par4 
			WHERE "DocEntry"=:POEntry13 AND "VisOrder"=:par5;*/ -- Update PO Line Status

			/*UPDATE TRIWALL_TRAINKEY."INV1"	
				SET "U_ODraftQty"=(CAST(:par3 AS NUMERIC)+Cast(@Qty13 AS NUMERIC))
			WHERE "DocEntry"=:POEntry13 AND "VisOrder"=:par5;*/

			SELECT 
				COUNT("ItemCode") INTO Close13 
			FROM TRIWALL_TRAINKEY."INV1" 
			WHERE 
				"DocEntry"=:POEntry13 
				/*AND IFNULL("U_oDraftStatus",'')<>'C'*/;--Check status for update Document Header PO
			
			IF :Close13>0 THEN
				/*UPDATE TRIWALL_TRAINKEY."OINV" 
					SET "U_docDraft"='' 
				WHERE "DocEntry"=:POEntry13;*/
			END IF;
			
			IF :Close13=0 THEN
				/*UPDATE TRIWALL_TRAINKEY."OINV" 
					SET "U_docDraft"='oDraft' 
				WHERE "DocEntry"=:POEntry13;*/
			END IF;
			
			SELECT 
				"DocNum" 
			FROM TRIWALL_TRAINKEY."OINV" 
			WHERE "DocNum"=:par2;
		
		IF :par1='SNB' THEN
		--Serial Batch Return
			Declare CloseSN INT;
			Declare POEntrySN INT;
			Declare OutSerial Nvarchar(5000);

			SELECT 
				"DocEntry" INTO POEntrySN 
			FROM TRIWALL_TRAINKEY."ODLN" 
			WHERE "DocNum"=:par2;
			
			SELECT 
				IFNULL(''|| Cast(/*"U_OutSerial"*/'' As nvarchar(5000)),'') INTO OutSerial 
			FROM TRIWALL_TRAINKEY."DLN1" 
			WHERE 
				"DocEntry"=:POEntrySN 
				AND "VisOrder"=:par3;

			/*UPDATE TRIWALL_TRAINKEY."DLN1" 
				SET "U_OutSerial"=CASE WHEN :OutSerial='' THEN ''ELSE :OutSerial||',' END + :par4 
			WHERE 
				"DocEntry"=:POEntrySN 
				AND "VisOrder"=:par3;*/-- Update PO Line Status
			SELECT 'Ok' AS "Ok" FROM DUMMY;
		IF :par1='SNBAR' THEN
		--Serial Batch Return
			Declare CloseSNAR INT;
			Declare POEntrySNAR INT;
			Declare OutSerialAR Nvarchar(5000);

			SELECT 
				"DocEntry" INTO POEntrySNAR 
			FROM TRIWALL_TRAINKEY."OINV" 
			WHERE TO_VARCHAR("DocNum")=:par2;
			
			SELECT 
				IFNULL(''||Cast(/*"U_OutSerial"*/ '' As nvarchar(5000)),'') INTO OutSerialAR 
			FROM TRIWALL_TRAINKEY."INV1" 
			WHERE 
				"DocEntry"=:POEntrySNAR 
				AND "VisOrder"=:par3;
			/*UPDATE TRIWALL_TRAINKEY."INV1" 
				SET "U_OutSerial"=CASE WHEN :OutSerialAR='' THEN '' ELSE :OutSerialAR ||',' END || :par4 
			WHERE "DocEntry"=:POEntrySNAR And "VisOrder"=:par3;*/ -- Update PO Line Status
			SELECT 'Ok' AS "Ok" FROM DUMMY;
		END IF;
		END IF;
		END IF;
		END IF;
		END IF;
	ELSE IF :DTYPE='GetItemBrand' THEN
		SELECT 
			 "ItmsTypCod" As "Code"
			,"ItmsGrpNam" As "Name" 
		FROM TRIWALL_TRAINKEY."OITG" 
		ORDER BY "Code";
	ELSE IF :DTYPE='GetBarCode' THEN
		SELECT 
			 A."ItemCode"
			,A."ItemName"
			,C."ItmsGrpNam"
			,A."CodeBars"
		FROM TRIWALL_TRAINKEY."OITM" A 
		LEFT JOIN TRIWALL_TRAINKEY."KPPT" B ON B."ItemCode"=A."ItemCode"
		LEFT JOIN TRIWALL_TRAINKEY."OITG" C ON B."ItmsTypCod"=C."ItmsTypCod"
		WHERE 
			IFNULL(A."CodeBars",'')<>'' 
			And C."ItmsTypCod" IN(SELECT * FROM LIBRARY:SPLIT_TO_TABLE(:par1,','));
	ELSE IF :DTYPE='GetItemBarCode' THEN
		SELECT  
			 A."ItemCode"
			,A."ItemName"
			,C."ItmsGrpNam"
			,'*'|| A."CodeBars" ||'*' As "BarCode"
		FROM TRIWALL_TRAINKEY."OITM" A 
		LEFT JOIN TRIWALL_TRAINKEY."KPPT" B ON A."ItemCode"=B."ItemCode"
		LEFT JOIN TRIWALL_TRAINKEY."OITG" C ON B."ItmsTypCod"=C."ItmsTypCod"
		WHERE 
			IFNULL(A."CodeBars",'')<>'' 
			AND A."ItemCode" IN (SELECT * FROM LIBRARY:SPLIT_TO_TABLE(:par1,','));
	ELSE IF :DTYPE='GetItemInventoryTransferBranchToBranch' THEN
	
		CREATE LOCAL TEMPORARY TABLE #TMP("WhsCode" NVARCHAR(255));
		CREATE LOCAL TEMPORARY TABLE #TMP1(
			 "ItemCode" NVARCHAR(5000)
			,"OnHand" INT
		);
		CREATE LOCAL TEMPORARY TABLE #TMP2(
			 "ItemCode" NVARCHAR(5000)
			,"ItemName" NVARCHAR(5000)
			,"CostingPrice" FLOAT
			,"QtyinStock" INT
			,"ItemType" NVARCHAR(255)
		);
		
		SELECT 
			"WhsCode"
		FROM TRIWALL_TRAINKEY."OWHS" 
		WHERE 
			TO_VARCHAR("BPLid")=CASE WHEN :par1='' THEN TO_VARCHAR("BPLid") ELSE :par1 END
		INTO #TMP;
		
		SELECT 
			 "ItemCode"
			,SUM("OnHand") AS "OnHand" 
		FROM TRIWALL_TRAINKEY."OITW" 
		WHERE 
			"OnHand"<>0 
			AND "WhsCode" IN (SELECT "WhsCode" FROM #TMP) 
		GROUP BY "ItemCode" INTO #TMP1;
		SELECT 
			 A."ItemCode" AS "ItemCode"
			,IFNULL(A."ItemName",'' )AS "ItemName"
			,CAST(IFNULL(B."Price",0) AS float) AS "CostingPrice"
			,IFNULL(CAST(C."OnHand" As int),0) As "QtyinStock"
			,CASE WHEN A."ManBtchNum"='Y' THEN
				'B'
			 WHEN A."ManSerNum"='Y' THEN
				'S'
			 ELSE
				'N'
			 END AS "ItemType"
		FROM TRIWALL_TRAINKEY."OITM" AS A 
		LEFT JOIN TRIWALL_TRAINKEY."ITM1" AS B ON A."ItemCode"=B."ItemCode" AND B."PriceList"='1'
		RIGHT JOIN #TMP1 AS C ON A."ItemCode"=C."ItemCode"
		WHERE 
			A."validFor"='Y' 
			AND A."SellItem"='Y' 
			AND A."OnHand"<>0  
		INTO #TMP2;

		SELECT 
			* 
		FROM #TMP2 
		WHERE 
			"QtyinStock"<>0
		ORDER BY "ItemCode" DESC;
		/*OFFSET cast(@par2 as int) ROWS
		FETCH NEXT cast(@par3 as int) ROWS ONLY;*/
		DROP TABLE #TMP;
		DROP TABLE #TMP1;
		DROP TABLE #TMP2;
	ELSE IF :DTYPE='GetItemInventoryTransferBranchToBranch_Count' THEN
		CREATE LOCAL TEMPORARY TABLE #TMP3(
			"WhsCode" NVARCHAR(5000)
		);
		CREATE LOCAL TEMPORARY TABLE #TMP4(
			 "ItemCode" NVARCHAR(5000)
			,"OnHand" INT
		);
		CREATE LOCAL TEMPORARY TABLE #TMP5(
			 "ItemCode" NVARCHAR(5000)
			,"QtyinStock" INT
		);
	
		SELECT 
			"WhsCode"  
		FROM TRIWALL_TRAINKEY."OWHS" 
		WHERE 
			TO_VARCHAR("BPLid")=CASE WHEN :par1='' THEN TO_VARCHAR("BPLid") Else :par1 End
		INTO #TMP3;
		
		SELECT 
			 "ItemCode"
			,SUM("OnHand") As "OnHand" 
		FROM TRIWALL_TRAINKEY."OITW" 
		WHERE 
			"OnHand"<>0
			AND "WhsCode" IN (SELECT "WhsCode" FROM #TMP3) 
		GROUP BY "ItemCode" INTO #TMP4;
		
		SELECT 
			 A."ItemCode" AS "ItemCode"
			,IFNULL(CAST(C."OnHand" AS INT),0) AS "QtyinStock"
		FROM TRIWALL_TRAINKEY."OITM" AS A 
		LEFT JOIN TRIWALL_TRAINKEY."ITM1" AS B ON A."ItemCode"=B."ItemCode" AND B."PriceList"='1'
		RIGHT JOIN #TMP4 C ON A."ItemCode"=C."ItemCode"
		WHERE 
			A."validFor"='Y' 
			AND A."SellItem"='Y' 
			AND A."OnHand"<>0
		INTO #tmp5;
		
		SELECT 
			COUNT("ItemCode") AS "Count" 
		FROM #TMP5 
		WHERE "QtyinStock"<>0;
		
		DROP TABLE #TMP3;
		DROP TABLE #TMP4;
		DROP TABLE #TMP5;
	ELSE IF :DTYPE='GetSerailorBatchAvalableInventory' THEN
		IF :par1='S' THEN
			SELECT 
				 T0."ItemCode"
				,T5."DistNumber" As "SerialOrBatch"
				,CAST(T3."OnHandQty" As int) As Qty
				,TO_VARCHAR(T5."SysNumber") AS "SysNumber"
				,TO_VARCHAR(T5."InDate",'dd-MM-yyyy') As "AdmissionDate"
				,IFNULL(TO_VARCHAR(T4."ExpDate",'dd-MM-yyyy'),'') AS "ExpDate"
				,IFNULL(T4."LotNumber",'') AS "LotNumber"
			FROM TRIWALL_TRAINKEY."OIBQ" AS T0
			INNER JOIN TRIWALL_TRAINKEY."OBIN" T1 ON T0."BinAbs" = T1."AbsEntry" AND T0."OnHandQty" <> 0
			LEFT OUTER JOIN TRIWALL_TRAINKEY."OBBQ" AS T2 ON T0."BinAbs" = T2."BinAbs" AND T0."ItemCode" = T2."ItemCode" AND T2."OnHandQty" <> 0
			LEFT OUTER JOIN TRIWALL_TRAINKEY."OSBQ" AS T3 ON T0."BinAbs" = T3."BinAbs" AND T0."ItemCode" = T3."ItemCode" AND T3."OnHandQty" <> 0
			LEFT OUTER JOIN TRIWALL_TRAINKEY."OBTN" AS T4 ON T2."SnBMDAbs" = T4."AbsEntry" AND T2."ItemCode" = T4."ItemCode"
			LEFT OUTER JOIN TRIWALL_TRAINKEY."OSRN" AS T5 ON T3."SnBMDAbs" = T5."AbsEntry" and T3."ItemCode" = T5."ItemCode"
			WHERE
				T1."AbsEntry" >= 0 
				AND (T3."AbsEntry" IS NOT NULL)
				AND T0."ItemCode" in((
					SELECT 
						U0."ItemCode" 
					FROM TRIWALL_TRAINKEY."OITM" U0 
					INNER JOIN TRIWALL_TRAINKEY."OITB" U1 ON U0."ItmsGrpCod" = U1."ItmsGrpCod"
					WHERE U0."ItemCode" IS NOT NULL 
				))
				AND T0."ItemCode" IN (SELECT * FROM LIBRARY:SPLIT_TO_TABLE(:par2,','));
		ELSE IF :par1='B' THEN
			SELECT 
				 A."ItemCode" AS "ItemCode"
				,A."DistNumber" AS "SerialOrBatch"
				,CAST(B."Quantity" As int) AS "Qty"
				,TO_VARCHAR(A."SysNumber") AS "SysNumber"
				,IFNULL(A."LotNumber",'') AS "LotNumber"
				,IFNULL(TO_VARCHAR(A."InDate"),'')AS "AdmissionDate"
				,IFNULL(TO_VARCHAR(A."ExpDate"),'') AS "ExpDate"
				,IFNULL(A."LotNumber",'') AS "LotNumber"
			FROM TRIWALL_TRAINKEY."OBTN" AS A
			LEFT JOIN TRIWALL_TRAINKEY."OBTQ" AS B ON A."ItemCode"=B."ItemCode" AND A."SysNumber"=B."SysNumber"
			WHERE 
				A."ItemCode"=:par2 
				AND B."Quantity"!=0;
		END IF;
		END IF;
	ELSE IF :DTYPE='GetBranchAndDftWarehouse'THEN
		SELECT 
			 "BPLId" AS "BranchID"
			,"BPLName" AS "BranchName"
			,"DfltResWhs" AS "Warehouse"
		FROM TRIWALL_TRAINKEY."OBPL" 
		WHERE "Disabled"!='Y';
	ELSE IF :DTYPE='GetInventoryTransferBranchToBranchList' THEN
		--SELECT 
		--	 A."DocEntry" AS "DocEntry"
		--	,IFNULL(TO_VARCHAR(A."U_DocDate",'dd/MM/yyyy'),'') AS "DocDate"
		--	,/*A."U_CreateBy"*/'' AS CreateBy
		--	,/*A."U_Approve"*/'' AS Approve
		--	,/* B.BPLName */'' AS BranchFrom		
		--	,/*BB."BPLName"*/'' AS BranchTo
		--FROM TRIWALL_TRAINKEY."@TB_OTRF_B_2_B" AS A
		--LEFT JOIN TRIWALL_TRAINKEY."OBPL" AS B ON B."BPLId"=A."U_BranchFrom"
		--LEFT JOIN TRIWALL_TRAINKEY."OBPL" AS BB ON BB."BPLId"=A."U_BranchTo"
		--WHERE "Status"='O' AND A."U_Approve"='Pending'
		--ORDER BY "DocEntry" DESC;
		/*OFFSET cast(@par3 as int) ROWS
		FETCH NEXT cast(@par4 as int) ROWS ONLY*/
		SELECT 'CommingSoon' AS "Test" FROM DUMMY;
	ELSE IF :DTYPE='GetInventoryTransferBranchToBranchListCount' THEN
		/*SELECT 
			 COUNT(A."DocEntry") AS Count
		FROM TRIWALL_TRAINKEY."@TB_OTRF_B_2_B" AS A
		LEFT JOIN TRIWALL_TRAINKEY."OBPL" AS B ON B."BPLId"=A."U_BranchFrom"
		LEFT JOIN TRIWALL_TRAINKEY."OBPL" AS BB ON BB."BPLId"=A."U_BranchTo"
		WHERE "Status"='O' AND A."U_Approve"='Pending';*/
		SELECT 'CommingSoon' AS "Test" FROM DUMMY;
	ELSE IF :DTYPE='GetInventoryTransferBranchToBranchHeaderByDocEntry' THEN
		/*
		SELECT 
			 A."DocEntry" AS "DocEntry"
			,A."U_DocDate" AS "DocDate"
			,A."U_Ref2" AS "Ref2"
			,B."BPLName" AS "BranchFrom"		
			,BB."BPLName" AS "BranchTo"
			,A."U_Remarks" AS "Remarks"
		FROM TRIWALL_TRAINKEY."@TB_OTRF_B_2_B" AS A
		LEFT JOIN TRIWALL_TRAINKEY."OBPL" AS B ON B."BPLId"=A."U_BranchFrom"
		LEFT JOIN TRIWALL_TRAINKEY."OBPL" AS BB ON BB."BPLId"=A."U_BranchTo"
		WHERE "Status"='O' AND A."DocEntry"=:par1;
		*/
		SELECT 'CommingSoon' AS "Test" FROM DUMMY;
	ELSE IF :DTYPE='GetInventoryTransferBranchToBranchLineByDocEntry' THEN
		/*
		SELECT 
			 A."U_ItemCode" AS "ItemCode"
			,A."U_ItemName" AS "ItemName"
			,CAST(A."U_Qty" AS INT) AS "Qty"
			,CAST(A."U_Qty" AS float) AS "TotalCostingPrice"
			,CAST(A."U_CostingPrice" AS float) AS "CostingPrice"
			,A."U_ItemType" AS "ItemType"
			,A."U_Serail_Batch" AS "Serail_Batch"
		FROM TRIWALL_TRAINKEY."@TB_TRF1_B_2_B" AS A
		WHERE A."DocEntry"=:par1;
		*/
		SELECT 'CommingSoon' AS "Test" FROM DUMMY;
	ELSE IF :DTYPE='GetLayoutInventoryDraft' THEN
		/*
		SELECT 
			 ROW_NUMBER() OVER(ORDER BY "ItemCode") AS "No"
			,A."U_ItemCode" AS "ItemCode"
			,A."U_ItemCode" AS "ItemName"
			,CAST(A."U_Qty" AS INT) AS "Qty"
			,CAST(A."U_Qty" AS float) AS "TotalCostingPrice"
			,CAST(A."U_CostingPrice" AS float) AS "CostingPrice"
			,'50107-199' AS "AccountCode"
			,CASE WHEN B."ManBtchNum"='Y' THEN 'B'
			 WHEN B."ManSerNum"='Y' THEN 'S'
			 ELSE 'N' END AS "ItemType"
			,A."DocEntry" AS "DocEntry"
			,A."U_Serail_Batch" AS "SerailBatch"
			,B."InvntryUom" AS "UoMCode"
			,TO_VARCHAR(A1."U_DocDate", 'MMM/dd/yyyy') AS "UoMCode"
			,A1."U_BranchTo" AS "Branchto"
			,A1."U_BranchFrom" As "BranchFrom"
			,IFNULL(A1."U_Remarks",'') AS "Remarks"
		FROM TRIWALL_TRAINKEY."@TB_TRF1_B_2_B" AS A
		Left JOIN TRIWALL_TRAINKEY."@TB_OTRF_B_2_B" AS A1 ON A1."DocEntry"=A."DocEntry"
		LEFT JOIN TRIWALL_TRAINKEY."OITM" AS B ON A."U_ItemCode"=B."ItemCode"
		WHERE A."DocEntry"=:par1;
		*/
		SELECT 'CommingSoon' AS "Test" FROM DUMMY;
	ELSE IF :DTYPE='GetItemBOMs' THEN
		SELECT DISTINCT 
			 A."Code" AS "Code"
			,B."ItemName" AS "ItemName"
			,B."FrgnName" As "ItemNameKH"
			,CAST(IFNULL(F."OnHand", 0) AS FLOAT) "OnHand"
			,E."UomName" AS "Uom"
			,IFNULL(A."ToWH", '') "Warehouse"
		FROM TRIWALL_TRAINKEY."OITT" A
		INNER JOIN TRIWALL_TRAINKEY."OITM" B ON A."Code"=B."ItemCode"
		LEFT JOIN TRIWALL_TRAINKEY."OWHS" C ON A."ToWH" = C."WhsCode"
		LEFT JOIN TRIWALL_TRAINKEY."NNM1" D ON D."BPLId" = C."BPLid"
		LEFT JOIN TRIWALL_TRAINKEY."OITW" F ON B."ItemCode" = F."ItemCode" AND F."WhsCode" = C."WhsCode"
		LEFT JOIN TRIWALL_TRAINKEY."OUOM" E ON E."UomEntry" = B."UgpEntry"
		WHERE D."Series" = :par2
		AND A."TreeType" = 'P';
		--SELECT * FROM EW_PRD_T2."OWHS"
	ELSE IF :DTYPE='GetItemBOMInfo' THEN
		SELECT 
			 A."Code"
			,B."ItemName"
			,C."UomName" AS "Uom"
			,A."ToWH" "Warehouse"
		FROM TRIWALL_TRAINKEY."OITT" A
			LEFT JOIN TRIWALL_TRAINKEY."OITM" B ON A."Code"=B."ItemCode"
			LEFT JOIN TRIWALL_TRAINKEY."OUOM" C ON B."UgpEntry" = C."UomEntry"
		WHERE "Code"=:par1;
	ELSE IF :DTYPE='GetListWarehouses' THEN	
		SELECT 
			 "WhsCode" AS "Code"
			,OBPL."BPLName" "BranchName"
		FROM TRIWALL_TRAINKEY."OWHS" 
		LEFT JOIN TRIWALL_TRAINKEY."OBPL" ON OWHS."BPLid" = OBPL."BPLId" 
		LEFT JOIN TRIWALL_TRAINKEY."NNM1" ON NNM1."BPLId" = OBPL."BPLId"
		WHERE NNM1."Series" = :par1
		Order By "WhsCode";
	ELSE IF :DTYPE='GetListUsers' THEN
		SELECT 
			 "USERID" AS "Id"
			,IFNULL("U_NAME", "USER_CODE") AS "Name" 
		FROM TRIWALL_TRAINKEY."OUSR" 
		Order By "U_NAME";
	ELSE IF :DTYPE='GetItemBOMsDetail' THEN
		SELECT 
			 C."Type" AS "ItemType"
			,C."Code" "ItemCode"
			,CASE WHEN C."Type"!=4 THEN E."ResName" ELSE D."ItemName" END "ItemName"
			,CAST(C."Quantity" AS FLOAT) "BaseQty"
			,CAST(C."Quantity" AS FLOAT) "PlannedQty"
			,CAST(0 AS FLOAT) "IssuedQty"
			,CAST((IFNULL(F."OnHand" + F."OnOrder" - F."IsCommited", 0)) AS FLOAT) "OnOrder"
			,IFNULL(D."InvntryUom", '') "InvntryUom"
			,IFNULL(C."Warehouse", 'WH13') "Warehouse"
			,C."IssueMthd" "IssueType"
			,CAST(IFNULL(D."OnHand",0) AS FLOAT) "OnHand"
			,IFNULL(H."BPLName", '') "Branch"
		FROM TRIWALL_TRAINKEY."OITT" A
			LEFT JOIN TRIWALL_TRAINKEY."OITM" B ON A."Code"=B."ItemCode"
			LEFT JOIN TRIWALL_TRAINKEY."ITT1" C ON C."Father"=A."Code"
			LEFT JOIN TRIWALL_TRAINKEY."OITM" D ON D."ItemCode"=C."Code"
			LEFT JOIN TRIWALL_TRAINKEY."OITW" F ON D."ItemCode" = F."ItemCode" AND F."WhsCode" = A."ToWH"
			LEFT JOIN(
				SELECT 
					 "ResCode"
					,"ResName" 
				FROM TRIWALL_TRAINKEY."ORSC"
			) E ON E."ResCode"=C."Code"
			LEFT JOIN TRIWALL_TRAINKEY."OWHS" G ON G."WhsCode" = C."Warehouse"
			LEFT JOIN TRIWALL_TRAINKEY."OBPL" H ON H."BPLId" = G."BPLid"
		WHERE A."Code"=:par1;
	ELSE IF :DTYPE='GetAllListGoodReceiptDraf' THEN
		SELECT 
			 "DocEntry" As "DocEntry"
			,"DocNum" AS "DocNum"
			,TO_VARCHAR("DocDate",'dd-MM-yyyy')AS "PostingDate"
			,"CardCode" AS "VendorCode"
			,CAST("DocTotal" AS FLOAT)AS "DocTotal"
		FROM TRIWALL_TRAINKEY."ODRF"
		WHERE "DocStatus"='O' /*And "U_docDraft"<>'O'*/ And "ObjType"='20'
		ORDER BY "DocEntry" DESC;
		/*OFFSET CAST(:par1 AS INT) ROWS
		FETCH NEXT cast(@par2 as int) ROWS ONLY*/
	ELSE IF :DTYPE='GET_AllList_GoodReceiptDraft_Count' THEN
		SELECT 
			COUNT("DocNum") AS "Count" 
		FROM TRIWALL_TRAINKEY."ODRF" 
		WHERE "DocStatus"='O' /*And "U_docDraft"<>'O'*/ And "ObjType"='20';
	ELSE IF :DTYPE='GetHeaderGoodReceiptpoDrafbyDocEntry' THEN
		SELECT DISTINCT
			 A."DocEntry" AS "DocEntry"
			,A."CardCode" AS "CardCode"
			,A."CardName" AS "CardName"
			,TO_VARCHAR(A."DocDate",'dd-MM-yyyy' ) AS "PostingDate"
			,TO_VARCHAR(A."DocDueDate",'dd-MM-yyyy' )AS "DueDate"
			,A."DocNum"  AS "DocNum"
			,B1."BPLName" AS "Branch"
			,A."Comments" AS "Remarks"
			,IFNULL(C."Name",'')AS "ContactPerson"
			,IFNULL(A."NumAtCard",'' )AS "Ref-No"
			,D."SeriesName" AS "SeriesName"
		 FROM TRIWALL_TRAINKEY."ODRF" A
		 LEFT JOIN TRIWALL_TRAINKEY."DRF1" AS B ON B."DocEntry"=A."DocEntry"
		 LEFT JOIN TRIWALL_TRAINKEY."OBPL" AS B1 ON B1."BPLId"=A."BPLId"
		 LEFT JOIN TRIWALL_TRAINKEY."OCPR" AS C ON C."CardCode"=A."CardCode" And C."CntctCode"=A."CntctCode"
		 LEFT JOIN TRIWALL_TRAINKEY."NNM1" AS D On D."Series"=A."Series" and D."ObjectCode"=20
		 Where 
		 	A."DocStatus"='O' 
		 	--AND A."U_docDraft"<>'O'  
		 	AND A."ObjType"='20' 
		 	AND A."DocEntry"=:par1;
	ELSE IF :DTYPE='GetLineGoodreceiptpoDrafByDocEntry' THEN
		SELECT 
			 A."ItemCode" AS "ItemCode"
			,A."Dscription" AS "ItemName"
			,IFNULL(A."CodeBars",'' )AS "BarCode"
			,CAST(A."Quantity" AS INT)AS "Quantity"
			,CAST(A."Price" AS FLOAT ) AS "UnitPrice"
			,A."WhsCode" AS "WhsCode"
			,A."VatGroup" AS "VAT"
			,CAST(A."LineTotal" AS FLOAT)AS "LineTotal"
			,A."UomCode" AS "UoM"
			,CASE WHEN B."ManBtchNum"='Y' THEN
				'B'
			 WHEN B."ManSerNum"='Y' THEN
				'S'
			 ELSE
				'N'
			 END AS "ManageItem"
		FROM TRIWALL_TRAINKEY."DRF1" A
		LEFT JOIN TRIWALL_TRAINKEY."OITM" AS B ON B."ItemCode"=A."ItemCode"
		WHERE
			A."LineStatus"='O' 
			AND A."ObjType"='20' 
			AND A."DocEntry"=:par1;
	ELSE IF :DTYPE='GetListGoodReturnDetail' THEN
		SELECT 
			 "DocEntry" AS "DocEntry"
			,"CardCode" AS "CardCode"
			,"CardName" AS "CardName"
			,TO_VARCHAR("DocDate",'dd-MM-yyyy' )AS "PostingDate"
			,CAST("DocTotal" AS FLOAT)AS "DocTotal"
		 FROM TRIWALL_TRAINKEY."ODRF"
		 WHERE "DocStatus"='O' and "ObjType"='21'
		 ORDER BY "DocEntry" DESC;
		/*OFFSET cast(@par1 as int) ROWS
		FETCH NEXT cast(@par2 as int) ROWS ONLY*/
	ELSE IF :DTYPE='GetListGoodReturnDetail_Count' THEN
		SELECT 
			COUNT("DocEntry") AS "Count" 
		FROM TRIWALL_TRAINKEY."ODRF" 
		WHERE "DocStatus"='O' and "DocStatus"='21';
	ELSE IF :DTYPE='GetListGoodReturnAddDraft' THEN
		SELECT 
			 "DocEntry" AS "DocEntry"
			,"CardCode" AS "CardCode"
			,"CardCode" AS "CardName"
			,TO_VARCHAR("DocDate",'dd-MM-yyyy' )AS "PostingDate"
			,CAST("DocTotal" AS FLOAT)AS "DocTotal"
		 FROM TRIWALL_TRAINKEY."ODRF"
		 WHERE
		 "DocStatus"='O' 
		 AND "ObjType"='21';
		 --AND "U_docDraft"=:par1
	ELSE IF :DTYPE='GetHeaderGoodsReturnByDocEntry' THEN
		SELECT 
			 A."DocEntry" AS "DocEntry"
			,A."CardCode" AS "CardCode"
			,A."CardName" AS "CardName"
			,TO_VARCHAR(A."DocDate",'dd-MMM-yyyy' ) AS "PostingDate"
			,TO_VARCHAR(A."DocDueDate",'dd-MMM-yyyy' )AS "DueDate"
			,A."DocNum"  AS "DocNum"
			,B1."BPLName" AS "Branch"
			,A."Comments" AS "Remarks"
			,IFNULL(C."Name",'')AS "ContactPerson"
			,IFNULL(A."NumAtCard",'' )AS "RefNo"
			,D."SeriesName" AS "SeriesName"
		 FROM TRIWALL_TRAINKEY."ODRF" A
		 LEFT JOIN TRIWALL_TRAINKEY."OBPL" AS B1 ON B1."BPLId"=A."BPLId"
		 LEFT JOIN TRIWALL_TRAINKEY."OCPR" AS C ON C."CardCode"=A."CardCode" And C."CntctCode"=A."CntctCode"
		 LEFT JOIN TRIWALL_TRAINKEY."NNM1" AS D On D."Series"=A."Series" and D."ObjectCode"=21
		 WHERE 
		 	A."ObjType"='21' 
		 	AND A."DocEntry"=:par1;
	ELSE IF :DTYPE='GetLineItemGoodsReturnByDOcEntry' THEN
		SELECT 
			 A."ItemCode" AS "ItemCode"
			,A."Dscription" AS "ItemName"
			,IFNULL(A."CodeBars",'') AS "BarCode"
			,CAST(A."Quantity" AS INT)AS "QTY"
			,CAST(A."Price" AS FLOAT) AS "UnitPrice"
			,A."VatGroup" AS "VAT"
			,CAST(A."LineTotal" AS FLOAT) AS "LineTotal"
			,CASE WHEN B."ManBtchNum"='Y' THEN
				'B'
			 WHEN B."ManSerNum"='Y' THEN
				'S'
			 ELSE
				'N'
			 END AS "ManageItem"
			,CASE WHEN A."LineStatus"='C' THEN
				'Complite'
			 WHEN A."LineStatus"='O' THEN
				'Pandding'
			 END AS "Type"
		FROM TRIWALL_TRAINKEY."DRF1" A
		LEFT JOIN TRIWALL_TRAINKEY."OITM" AS B ON B."ItemCode"=A."ItemCode"
		WHERE 
			A."ObjType"='21' 
			AND A."DocEntry"=:par1;
	ELSE IF :DTYPE='GetListDelivery' THEN
		SELECT 
			 "DocEntry" AS "DocEntry"
			,"CardCode" AS "VendorCode"
			,"CardName" AS "VendorName"
			,CAST("DocTotal" AS FLOAT)AS "DocTotal" 
			,TO_VARCHAR("DocDate",'dd-MM-yyyy') AS "PostingDate"
		FROM TRIWALL_TRAINKEY."ODLN" 
		WHERE 
			"DocStatus"='O'
			--AND U_WebID<>'' 
			AND "ObjType"=15 
			--AND IFNULL("U_docDraft",'')<>'oDraft'
			ORDER BY "DocEntry" DESC;
		/*OFFSET cast(@par1 as int) ROWS
		FETCH NEXT cast(@par2 as int) ROWS ONLY;*/
	ELSE IF :DTYPE='GetListDelivery_Count' THEN
		SELECT 
			COUNT("DocEntry") AS "Count" 
		FROM TRIWALL_TRAINKEY."ODLN"
		WHERE 
			 "DocStatus"='O'
			--AND U_WebID<>'' 
			AND "ObjType"=15;
			--AND IFNULL(U_docDraft,'')<>'oDraft'
	ELSE IF :DTYPE='GetDeliveryHeaderByDocEntry' THEN
		SELECT
			 A."DocEntry" AS "DocEntry"
			,A."CardCode" AS "CardCode"
			,A."CardName" AS "CardName"
			,TO_VARCHAR(A."DocDate",'dd-MM-yyyy' ) AS "PostingDate"
			,TO_VARCHAR(A."DocDueDate",'dd-MMM-yyyy' )AS "DueDate"
			,A."DocNum" AS "DocNum"
			,B1."BPLName" AS "Branch"
			,IFNULL(A."Comments",'') AS "Remarks"
			,IFNULL(/*A."U_DeliCon"*/'','' )AS "DeliveryCondition"
			,IFNULL(C."Name",'')AS "ContactPerson"
			,IFNULL(A."NumAtCard",'' )AS "RefNo"
			,D."SeriesName" AS "SeriesName"
		FROM TRIWALL_TRAINKEY."ODLN" A
		LEFT JOIN TRIWALL_TRAINKEY."OBPL" AS B1 ON B1."BPLId"=A."BPLId"
		LEFT JOIN TRIWALL_TRAINKEY."OCPR" AS C ON C."CardCode"=A."CardCode" And C."CntctCode"=A."CntctCode"
		LEFT JOIN TRIWALL_TRAINKEY."NNM1" AS D On D."Series"=A."Series" and D."ObjectCode"=15
		WHERE 
			A."DocStatus"='O' 
			AND A."ObjType"='15' 
			--AND "U_WebID"<>'' 
			--AND IFNULL("U_docDraft",'')<>'oDraft' 
			AND A."DocEntry"=:par1;
	ELSE IF :DTYPE='GetItemLineDeliveryDetailByDocEntry' THEN
		SELECT 
			 A."ItemCode" AS "ItemCode"
			,A."Dscription" AS "ItemName"
			,IFNULL(A."CodeBars",'') AS "BarCode"
			,CAST(A."Quantity" AS INT)AS "QTY"
			,CAST(A."Price" AS FLOAT) AS "UnitPrice"
			,A."VatGroup" AS "VAT"
			,CAST(A."LineTotal" AS FLOAT) AS "LineTotal"
			,CASE WHEN B."ManBtchNum"='Y' THEN
				'B'
			 WHEN B."ManSerNum"='Y' THEN
				'S'
			 ELSE
				'N'
			 END AS "ManageItem"
			,CASE WHEN A."LineStatus"='O' THEN
				'Complite'
			 END AS "Type"
		FROM TRIWALL_TRAINKEY."DLN1" A
		LEFT JOIN TRIWALL_TRAINKEY."OITM" AS B ON B."ItemCode"=A."ItemCode"
		WHERE 
			A."ObjType"='15' 
			AND A."LineStatus"='O' 
			AND A."DocEntry"=:par1;
	ELSE IF :DTYPE='GetallListofDeliveryReturn' THEN
		SELECT 
			 "DocEntry" AS "DocEntry"
			,"CardCode" AS "VendorCode"
			,"CardName" AS "VendorName"
			,TO_VARCHAR("DocDate",'dd-MMM-yyyy' )AS "PostingDate"
			,CAST("DocTotal" AS float)AS "DocTotal" 
		FROM TRIWALL_TRAINKEY."ODRF"
		WHERE 
			"DocStatus"='O' 
			AND "ObjType"='16' 
			--And U_WebID<>'' 
			--And IFNULL("U_docDraft",'')<>'oDraft' 
		ORDER BY "DocEntry" DESC;
		/*OFFSET cast(@par1 as int) ROWS
		FETCH NEXT cast(@par2 as int) ROWS ONLY;*/
	ELSE IF :DTYPE='GetallListofDeliveryReturn_Count' THEN
		SELECT 
			COUNT("DocEntry") AS "Count" 
		FROM TRIWALL_TRAINKEY."ODRF" 
		Where 
			"DocStatus"='O' 
			AND "ObjType"='16'; 
			--And "U_WebID"<>''
			--And IFNULL("U_docDraft",'')<>'oDraft'
	ELSE IF :DTYPE='GetHeaderDeliveryReturnByDOnEntry' THEN
		SELECT
				 A."DocEntry" AS "DocEntry"
				,A."CardCode" AS "CardCode"
				,A."CardName" AS "CardName"
				,TO_VARCHAR(A."DocDate",'dd-MM-yyyy') AS "PostingDate"
				,TO_VARCHAR(A."DocDueDate",'dd-MM-yyyy' )AS "DueDate"
				,A."DocNum"  AS "DocNum"
				,B1."BPLName" AS "Branch"
				,IFNULL(A."Comments",'') AS "Remarks"
				,IFNULL(C."Name",'')AS "ContactPerson"
				,IFNULL(A."NumAtCard",'' )AS "RefNo"
				,D."SeriesName" AS "SeriesName"
		 FROM TRIWALL_TRAINKEY."ODRF" A
		 LEFT JOIN TRIWALL_TRAINKEY."OBPL" AS B1 ON B1."BPLId"=A."BPLId"
		 LEFT JOIN TRIWALL_TRAINKEY."OCPR" AS C ON C."CardCode"=A."CardCode" And C."CntctCode"=A."CntctCode"
		 LEFT JOIN TRIWALL_TRAINKEY."NNM1" AS D On D."Series"=A."Series" and D."ObjectCode"=16
		 WHERE 
		 	A."DocStatus"='O' 
		 	AND A."ObjType"='16' 
		 	--AND "U_WebID"<>'' 
		 	AND A."DocEntry"=:par1;
	ELSE IF :DTYPE='GetLineDeliveryReturnByDocEntry' THEN
		SELECT DISTINCT	
			 A."ItemCode" AS "ItemCode"
			,A."Dscription" AS "ItemName"
			,IFNULL(A."CodeBars",'') AS "BarCode"
			,CAST(A."Quantity" AS INT)AS "QTY"
			,CAST(A."Price" AS FLOAT) AS "UnitPrice"
			,A."VatGroup" AS "VAT"
			,CAST(A."LineTotal" AS FLOAT) AS "LineTotal"
			,CASE WHEN B."ManBtchNum"='Y' THEN
				'B'
			 WHEN B."ManSerNum"='Y' THEN
				'S'
			 ELSE
				'N'
			 END AS "ManageItem"
			,CASE WHEN A."LineStatus"='O' THEN
				'Complite'
			 END AS "Type"				
		FROM TRIWALL_TRAINKEY."DRF1" A
		LEFT JOIN TRIWALL_TRAINKEY."OITM" AS B ON B."ItemCode"=A."ItemCode"
		WHERE 
			A."ObjType"='16' 
			AND A."LineStatus"='O' 
			AND A."DocEntry"=:par1;
	ELSE IF :DTYPE='GetAllListCraditMemo' THEN
		SELECT 
			 "DocEntry" AS "DocEntry"
			,"CardCode" AS "VendorCode"
			,"CardName" AS "VendorName"
			,CAST("DocTotal" AS float) AS "DocTotal" 
			,TO_VARCHAR("DocDate",'dd-MMM-yyyy') AS "PostingDate"
		FROM TRIWALL_TRAINKEY."ODRF"
		Where 
			"ObjType"=14 
			AND "DocStatus"='O' 
			AND "DocType"='I'
		 ORDER BY "DocEntry" DESC;
		 /*OFFSET cast(@par1 as int) ROWS
		 FETCH NEXT cast(@par2 as int) ROWS ONLY;*/
	ELSE IF :DTYPE='GetAllListCraditMemo_Count' THEN
		SELECT 
			COUNT("DocEntry") AS "Count" 
		FROM TRIWALL_TRAINKEY."ODRF" 
		Where 
			"ObjType"=14 
			AND "DocStatus"='O' 
			AND "DocType"='I';
	ELSE IF :DTYPE='GetHeaderCreditMemoByDocEntrys' THEN
		SELECT 
			 A."DocEntry" AS "DocEntry"
			,A."CardCode" AS "CardCode"
			,A."CardName" AS "CardName"
			,TO_VARCHAR(A."DocDate",'dd-MM-yyyy' ) AS "PostingDate"
			,TO_VARCHAR(A."DocDueDate",'dd-MM-yyyy' )AS "DueDate"
			,A."DocNum"  AS "DocNum"
			,B1."BPLName" AS "Branch"
			,IFNULL(A."Comments",'') AS "Remarks"
			,IFNULL(/*A."U_DeliCon"*/'','' )AS "DeliveryCondition"
			,IFNULL(C."Name",'') AS "ContactPerson"
			,IFNULL(A."NumAtCard",'' )AS "RefNo"
			,D."SeriesName" AS "SeriesName"
		 FROM TRIWALL_TRAINKEY."ODRF" A
		 LEFT JOIN TRIWALL_TRAINKEY."OBPL" AS B1 ON B1."BPLId"=A."BPLId"
		 LEFT JOIN TRIWALL_TRAINKEY."OCPR" AS C ON C."CardCode"=A."CardCode" And C."CntctCode"=A."CntctCode"
		 LEFT JOIN TRIWALL_TRAINKEY."NNM1" AS D On D."Series"=A."Series" and D."ObjectCode"=14
		WHERE 
			A."ObjType"=14
			AND A."DocStatus"='O' 
			AND A."DocType"='I' 
			AND A."DocEntry"=:par1;
	ELSE IF :DTYPE='' THEN
		SELECT 
			 A."ItemCode" AS "ItemCode"
			,A."Dscription" AS "ItemName"
			,IFNULL(A."CodeBars",'') AS "BarCode"
			,CAST(A."Quantity" AS INT)AS "QTY"
			,CAST(A."Price" AS FLOAT) AS "UnitPrice"
			,A."VatGroup" AS "VAT"
			,CAST(A."LineTotal" AS FLOAT) AS "LineTotal"
			,CASE WHEN B."ManBtchNum"='Y' THEN
				'B'
			 WHEN B."ManSerNum"='Y' THEN
				'S'
			 ELSE
				'N'
			 END AS "ManageItem"
			,CASE WHEN A."LineStatus"='O' THEN
				'Complite'
			 END AS "Type"
		FROM TRIWALL_TRAINKEY."DRF1" A
		LEFT JOIN TRIWALL_TRAINKEY."OITM" AS B ON B."ItemCode"=A."ItemCode"
		WHERE 
			A."LineStatus"='O' 
			AND A."ObjType"=14 
			AND A."DocEntry"=:par1;
	ELSE IF :DTYPE='GetUoM' THEN
		SELECT 
			 B."UomCode" As "UomGroup"
			,C."UomCode"
		FROM TRIWALL_TRAINKEY."UGP1" A 
		LEFT JOIN TRIWALL_TRAINKEY."OUOM" B ON A."UgpEntry"=B."UomEntry"
		LEFT JOIN TRIWALL_TRAINKEY."OUOM" C ON A."UomEntry"=C."UomEntry";
	ELSE IF :DTYPE='GetNextNumber' THEN
		IF :par1='67' THEN
			SELECT 
				"NextNumber" As "DocNum" 
			FROM TRIWALL_TRAINKEY."NNM1" 
			WHERE 
				"ObjectCode"=67 
				AND "Series"=:par2;
		IF :par1='16' THEN
			SELECT 
				"NextNumber" As "DocNum" 
			FROM TRIWALL_TRAINKEY."NNM1" 
			WHERE 
				"ObjectCode"=16 
				AND "Series"=:par2;
		IF :par1='15' THEN
			SELECT 
				"NextNumber" As "DocNum" 
			FROM TRIWALL_TRAINKEY."NNM1" 
			WHERE 
				"ObjectCode"=15 
				AND "Series"=:par2;
		IF :par1='14' THEN
			SELECT 
				"NextNumber" As "DocNum" 
			FROM TRIWALL_TRAINKEY."NNM1" 
			WHERE "ObjectCode"=14 And "Series"=:par2;
		IF :par1='1470000065' THEN
			SELECT 
				"NextNumber" As DocNum 
			FROM TRIWALL_TRAINKEY."NNM1" 
			WHERE 
				"ObjectCode"=1470000065 
				AND "Series"=:par2;
		IF :par1='59' THEN
			SELECT 
				"NextNumber" As "DocNum" 
			FROM TRIWALL_TRAINKEY."NNM1" 
			WHERE 
				"ObjectCode"=59 
				AND "Series"=:par2;
		END IF;
		END IF;
		END IF;
		END IF;
		END IF;
		END IF;
	ELSE IF :DTYPE='OCPR' THEN
		SELECT 
			 "CntctCode"
			,"Name" 
		FROM TRIWALL_TRAINKEY."OCPR" 
		WHERE "CardCode"=:par1;
	ELSE IF :DTYPE='Get_Availble_Serial' THEN
		SELECT 
			 T0."ItemCode" AS "ItemCode"
			,T5."DistNumber" As "SerialBatch"
			,CAST(T3."OnHandQty" AS INT) AS "Qty"
			,T1."WhsCode" AS "WhsCode"
			,T1."BinCode" AS "BinCode"
			,TO_VARCHAR(T5."ExpDate",'dd-MM-yyyy') As "ExpDate"
			,T5."SysNumber" AS "SysNumber"
			,T4."LotNumber" AS "LotNumber"
			,TO_VARCHAR(T5."MnfDate",'dd-MM-yyyy') As "MnfDate"
			,TO_VARCHAR(T5."InDate",'dd-MM-yyyy') As "AdmissionDate"
			,T5."MnfSerial" As "MfrNo"
		FROM
			TRIWALL_TRAINKEY."OIBQ" T0
			INNER JOIN TRIWALL_TRAINKEY."OBIN" T1 ON T0."BinAbs" = T1."AbsEntry" AND T0."OnHandQty" <> 0
			LEFT OUTER JOIN TRIWALL_TRAINKEY."OBBQ" T2 ON T0."BinAbs" = T2."BinAbs" AND T0."ItemCode" = T2."ItemCode" AND T2."OnHandQty" <> 0
			LEFT OUTER JOIN TRIWALL_TRAINKEY."OSBQ" T3 on T0."BinAbs" = T3."BinAbs" and T0."ItemCode" = T3."ItemCode" and T3."OnHandQty" <> 0
			LEFT OUTER JOIN TRIWALL_TRAINKEY."OBTN" T4 on T2."SnBMDAbs" = T4."AbsEntry" and T2."ItemCode" = T4."ItemCode"
			LEFT OUTER JOIN TRIWALL_TRAINKEY."OSRN" T5 on T3."SnBMDAbs" = T5."AbsEntry" and T3."ItemCode" = T5."ItemCode"
		WHERE
			T1."AbsEntry" >= 0 --and T1.WhsCode >= @WhsCode and T1.WhsCode <= @WhsCode 
			AND (T3."AbsEntry" IS NOT NULL)
			AND T0."ItemCode" IN(
				(
					SELECT 
						U0."ItemCode" 
					FROM TRIWALL_TRAINKEY."OITM" U0 INNER JOIN TRIWALL_TRAINKEY."OITB" U1 ON U0."ItmsGrpCod"=U1."ItmsGrpCod"
					WHERE U0."ItemCode" IS NOT NULL 
				))  
			AND T1."WhsCode" IN (SELECT * FROM LIBRARY:SPLIT_TO_TABLE(:par1,','))
			AND T0."ItemCode" IN (SELECT * FROM LIBRARY:SPLIT_TO_TABLE(:par2,','))
		UNION ALL
		SELECT 
			 A."ItemCode" AS "DistNumber"
			,B."DistNumber" AS "DistNumber"
			,A."Quantity" As "Qty"
			,A."WhsCode" AS "WhsCode"
			,'' As "BinCode"
			,B."ExpDate"
			,A."SysNumber"
			,B."LotNumber"
			,B."MnfDate"
			,B."InDate" As "AdmissionDate"
			,B."MnfSerial"
		FROM TRIWALL_TRAINKEY."OSRQ" A 
		LEFT JOIN TRIWALL_TRAINKEY."OSRN" B ON A."ItemCode"=B."ItemCode" AND A."SysNumber"=B."SysNumber" And A."MdAbsEntry"=B."AbsEntry"
		LEFT JOIN TRIWALL_TRAINKEY."OWHS" C ON C."WhsCode"=A."WhsCode" 
		WHERE 
			A."Quantity"<>0 
			AND A."ItemCode" IN (SELECT * FROM LIBRARY:SPLIT_TO_TABLE(:par2,',')) 
			AND A."WhsCode" IN (SELECT * FROM LIBRARY:SPLIT_TO_TABLE(:par1,','))
			AND C."BinActivat"='N'
		ORDER BY "ItemCode";
	ELSE IF :DTYPE='Get_Available_Batch' THEN
		SELECT 
			 "ItemCode" AS "ItemCode"
			,"BatchNum" As "SerialBatch"
			,"Quantity" As Qty
			,"WhsCode"
			,'' As "BinCode"
			,TO_VARCHAR("ExpDate",'dd-MM-yyyy') As "ExpDate" 
			,"SysNumber" 
			,'' As "LotNumber"
			,'' As "MnfDate"
			,TO_VARCHAR("InDate",'dd-MM-yyyy') As "AdmissionDate"
			,'' As "MfrNo"
		FROM TRIWALL_TRAINKEY."OIBT" WHERE "Direction"=0 And "Quantity"<>0 
		And "WhsCode" IN (SELECT * FROM LIBRARY:SPLIT_TO_TABLE(:par1,','))
		And "ItemCode" IN (SELECT * FROM LIBRARY:SPLIT_TO_TABLE(:par2,','));
	ELSE IF :DTYPE='TrfRequest_Status' THEN
		/*UPDATE 
			TRIWALL_TRAINKEY."OWTQ" 
		SET "U_TrnRequestSts"=:par1 
		Where "DocNum"=:par2;*/
		SELECT 'Ok' AS "Ok" FROM DUMMY;
	ELSE IF :DTYPE='OWTR_OSRN' THEN
		SELECT 
			 T1."ItemCode"
			,T4."IntrSerial" As "SerialBatch"
			,1 As "Qty"
			,T1."WhsCode"
			,'' As "BinCode"
			,TO_VARCHAR(T4."ExpDate",'dd-MM-yyyy') As "ExpDate"
			,T4."SysSerial" As "SysNumber"
			,T5."LotNumber"
			,TO_VARCHAR(T5."MnfDate",'dd-MM-yyyy') As "MnfDate"
			,TO_VARCHAR(T4."InDate",'dd-MM-yyyy') As "AdmissionDate"
			,T5."MnfSerial" As "MfrNo"
			,T3."BaseLinNum" As "DocLine"
		FROM TRIWALL_TRAINKEY."OWTR" T0 
		INNER JOIN TRIWALL_TRAINKEY."WTR1" T1 ON T0."DocEntry" = T1."DocEntry" 
		INNER JOIN TRIWALL_TRAINKEY."OITW" T2 ON T1."ItemCode" = T2."ItemCode" AND T1."WhsCode" = T2."WhsCode" 
		LEFT JOIN TRIWALL_TRAINKEY."SRI1" T3 ON T3."BaseEntry"=T1."DocEntry" AND T3."BaseLinNum"=T1."LineNum" AND T3."BaseType"='67' 
		LEFT JOIN TRIWALL_TRAINKEY."OSRI" T4 ON T3."ItemCode"=T4."ItemCode" AND T3."SysSerial" =T4."SysSerial" 
		LEFT JOIN TRIWALL_TRAINKEY."OSRN" T5 ON T5."ItemCode"=T4."ItemCode" And T4."SysSerial"=T5."SysNumber"
		WHERE 
			T3."Direction" = '1' 
			AND T0."DocNum"=:par1;
	ELSE IF :DTYPE='OWTR_OBTN' THEN 
		SELECT 
			 T0."ItemCode"
			,T4."DistNumber" As "SerialBatch"
			,T1."Quantity" As "Qty"
			,T0."LocCode" As "WhsCode"
			,'' As "BinCode"
			,T4."ExpDate"
			,T4."SysNumber"
			,T4."LotNumber"
			,TO_VARCHAR(T4."MnfDate",'dd-MMM-yyyy') As "MnfDate"
			,TO_VARCHAR(T4."InDate",'dd-MMM-yyyy') As "AdmissionDate"
			,T4."MnfSerial" As "MfrNo"
			,T0."DocLine"
		FROM TRIWALL_TRAINKEY."OITL" T0
		INNER JOIN TRIWALL_TRAINKEY."ITL1" T1 ON T1."LogEntry" = T0."LogEntry"
		INNER JOIN TRIWALL_TRAINKEY."OBTN" T4 ON T1."MdAbsEntry"=T4."AbsEntry"
		INNER JOIN TRIWALL_TRAINKEY."OITM" T2 ON T2."ItemCode" = T0."ItemCode"
		INNER JOIN TRIWALL_TRAINKEY."OINM" T5 ON T0."ItemCode" =T5."ItemCode" AND T0."LocCode"=T5."Warehouse" AND T0."AppDocNum" = T5."BASE_REF"
		WHERE 
			T0."DocNum"=:par1 
			AND T0."DocType"=67 
			AND T1."Quantity">0;
	ELSE IF :DTYPE='GetOWTRHeader' THEN
		SELECT 
			 "DocNum"
			,TO_VARCHAR("DocDate",'dd-MM-yyyy') As "DocDate"
			,"BPLId"
			,"BPLName"
			,"Filler" As "WhsFrom"
			,"ToWhsCode" As "WhsTo"
		FROM TRIWALL_TRAINKEY."OWTR" 
		WHERE 
			--"U_TrnRequestSts"='C' AND 
			"Series"=CASE WHEN :par1='' THEN "Series" ELSE :par1 END;
	ELSE IF :DTYPE='GetWTR1' THEN
		SELECT --
			 TO_VARCHAR(A."DocDate",'yyyy-MM-dd') As "DocDate"
			,TO_VARCHAR("ShipDate",'yyyy-MM-dd') As "ShipDate"			
			,IFNULL(A."CardCode",'') AS "CardCode"
			,IFNULL(A."CardName",'') AS "CardName"
			,IFNULL(A."CntctCode",'') AS "ContactCode"
			,IFNULL(A."Address",'') AS "Address"
			,IFNULL(A."JrnlMemo",'') As "Remark"
			,A."BPLId"
			,A."BPLName"
			,A."Filler" As "WhsFrom"
			,A."ToWhsCode" As "WhsTo"
			,A."DocEntry"
			,B."LineNum"
			,B."ItemCode"
			,"Dscription"
			,Cast("Quantity" As INT) As "Qty"
			,B."Price"
			,B."LineTotal"
			,"UomCode"
			,B."FromWhsCod" As "FromWhs"
			,B."WhsCode" As "ToWhs"
			,Case When C."ManBtchNum"='Y' Then 'Batch'
				  When C."ManSerNum"='Y' Then 'Serial'
				  ELSE 'None' End As "ItemType"
			,C."CodeBars" As "BarCode"
		FROM TRIWALL_TRAINKEY."OWTR" A
			LEFT JOIN TRIWALL_TRAINKEY."WTR1" B ON A."DocEntry"=B."DocEntry"
			LEFT JOIN TRIWALL_TRAINKEY."OITM" C ON B."ItemCode"=C."ItemCode"
			LEFT JOIN TRIWALL_TRAINKEY."OCPR" D ON A."CntctCode"=D."CntctCode"
		WHERE --A."U_TrnRequestSts"='C' And
		A."DocNum" IN(:par1);
	ELSE IF :DTYPE='GETBATCH_FOR_COUNTING' THEN
		SELECT
			 T0."ItemCode"
			,T4."DistNumber" As "SerialBatch"
			,T2."OnHandQty" As "Qty"
			,T1."WhsCode"
			,T1."BinCode"
			,TO_VARCHAR(T4."ExpDate",'dd-MM-yyyy') As "ExpDate"
			,T4."SysNumber"
			,T4."LotNumber"
			,T4."MnfDate"
			,'' As "MfrNo"
			,TO_VARCHAR(T4."InDate",'dd-MM-yyyy') As "AdmissionDate"
			,T0."BinAbs"
		FROM TRIWALL_TRAINKEY."OIBQ" T0
		INNER JOIN TRIWALL_TRAINKEY."OBIN" T1 on T0."BinAbs" = T1."AbsEntry" and T0."OnHandQty" <> 0
		left outer join TRIWALL_TRAINKEY."OBBQ" T2 on T0."BinAbs" = T2."BinAbs" and T0."ItemCode" = T2."ItemCode" and T2."OnHandQty" <> 0
		left outer join TRIWALL_TRAINKEY."OSBQ" T3 on T0."BinAbs" = T3."BinAbs" and T0."ItemCode" = T3."ItemCode" and T3."OnHandQty" <> 0
		left outer join TRIWALL_TRAINKEY."OBTN" T4 on T2."SnBMDAbs" = T4."AbsEntry" and T2."ItemCode" = T4."ItemCode"
		WHERE  T1."WhsCode" IN (SELECT * FROM LIBRARY:SPLIT_TO_TABLE(:par1,','))
			And T0."ItemCode" IN (SELECT * FROM LIBRARY:SPLIT_TO_TABLE(:par2,','))
			And T4."DistNumber"=:par3
			AND T0."BinAbs"=:par4;
	ELSE IF :DTYPE='GETSERIAL_FOR_COUNTING' THEN
		SELECT 
			 T0."ItemCode"
			,T5."DistNumber" As "SerialBatch"
			,Cast(T3."OnHandQty" As INT) As "Qty"
			,T1."WhsCode"
			,T1."BinCode"
			,T1."AbsEntry"
			,TO_VARCHAR(T5."ExpDate",'dd-MM-yyyy') As "ExpDate"
			,T5."SysNumber"
			,T4."LotNumber"
			,TO_VARCHAR(T5."MnfDate",'dd-MM-yyyy') As "MnfDate" 
			,TO_VARCHAR(T5."InDate",'dd-MM-yyyy') As "AdmissionDate"
			,T5."MnfSerial" As "MfrNo"
		FROM TRIWALL_TRAINKEY."OIBQ" T0
		INNER JOIN TRIWALL_TRAINKEY."OBIN" T1 on T0."BinAbs" = T1."AbsEntry" and T0."OnHandQty" <> 0
		LEFT OUTER JOIN TRIWALL_TRAINKEY."OBBQ" T2 on T0."BinAbs" = T2."BinAbs" and T0."ItemCode" = T2."ItemCode" and T2."OnHandQty" <> 0
		LEFT OUTER JOIN TRIWALL_TRAINKEY."OSBQ" T3 on T0."BinAbs" = T3."BinAbs" and T0."ItemCode" = T3."ItemCode" and T3."OnHandQty" <> 0
		LEFT OUTER JOIN TRIWALL_TRAINKEY."OBTN" T4 on T2."SnBMDAbs" = T4."AbsEntry" and T2."ItemCode" = T4."ItemCode"
		LEFT OUTER JOIN TRIWALL_TRAINKEY."OSRN" T5 on T3."SnBMDAbs" = T5."AbsEntry" and T3."ItemCode" = T5."ItemCode"
		WHERE
			T1."AbsEntry" >= 0 --and T1.WhsCode >= @WhsCode and T1.WhsCode <= @WhsCode 
			AND (T3."AbsEntry" IS NOT NULL)
			AND T0."ItemCode" IN ((SELECT U0."ItemCode" 
									FROM TRIWALL_TRAINKEY."OITM" U0 
									INNER JOIN TRIWALL_TRAINKEY."OITB" U1 ON U0."ItmsGrpCod" = U1."ItmsGrpCod"
									WHERE U0."ItemCode" IS NOT NULL 
			))  
			AND T5."DistNumber"=:par3
		ORDER BY T0."ItemCode";
	ELSE IF :DTYPE='GET_BP_Inventory_Transfer' THEN
		SELECT	TOP 200
				IFNULL(A."CardCode",'') AS "CardCode"
				,IFNULL(A."CardName" ,'') AS "CardName"
				,IFNULL(A0."Name",'') AS "ContactPerson"
				,IFNULL(IFNULL(A1."Address2"+', ',''),'') 
					+ IFNULL(IFNULL(A1."Address3"+', ',''),'') 
					+ IFNULL(A."City" ,'') 
					+  IFNULL(IFNULL(A."ZipCode"+', ',''),'') 
					+ IFNULL(IFNULL(A1."Street"+', ',''),'') 
					+ IFNULL(A2."Name",'') 
					+ IFNULL(A."Block",'') AS "ShipTo"
		FROM TRIWALL_TRAINKEY."OCRD" AS A
		LEFT JOIN TRIWALL_TRAINKEY."CRD1" AS A1 ON A1."CardCode"=A."CardCode" and A1."AdresType"='S'
		LEFT JOIN TRIWALL_TRAINKEY."OCPR" AS A0 ON A0."CardCode"=A."CardCode"
		LEFT JOIN TRIWALL_TRAINKEY."OCRY" AS A2 ON A2."Code"=A1."Country"
		WHERE A."CardType"='C' AND A."validFor"='Y';
	ELSE IF :DTYPE='GET_ITEM_Inventory_Transfer' THEN
		SELECT Top 500
			A."ItemCode" AS "ItemCode"
			,IFNULL(A."ItemName",'' )AS "ItemName"
			,CAST(A."OnHand" AS FLOAT) AS "QtyinStock"
			,CASE WHEN A."ManBtchNum"='Y' THEN
				'B'
				WHEN A."ManSerNum"='Y' THEN
				'S'
				ELSE
				'N'
				END AS "ItemType"
			,CAST(B."Price" as FLOAT)AS "UnitPrice"
			,IFNULL(A."InvntryUom",'') AS "UoM"
		FROM TRIWALL_TRAINKEY."OITM" AS A 
		LEFT JOIN TRIWALL_TRAINKEY."ITM1" AS B ON A."ItemCode"=B."ItemCode" AND B."PriceList"='1'
		WHERE 
			A."validFor"='Y' 
			AND A."SellItem"='Y' 
			AND A."OnHand"<>0;
	ELSE IF :DTYPE='GET_ITEM_LINE_TRANSFER' THEN
		SELECT	TOP 100
			 A."DocDate" AS "PostingDate"
			,A."DocDueDate" AS "DocumentDate"
			,A."BPLName" AS "FromBranch"
			,A."ToWhsCode" AS "ToWarehouse"
			,A."Filler" AS "FromWarehouse"
			,A."DocNum"
			,A0."SeriesName" AS "Series"
			,IFNULL(A."Comments",'') AS "Remarks"
			,IFNULL(A."ToBinCode",'') As "ToBinLocation"
			,A1."UomCode" AS "UoM"
		FROM TRIWALL_TRAINKEY."OWTR" AS A
		LEFT JOIN TRIWALL_TRAINKEY."WTR1" AS A1 ON A1."DocEntry"=A."DocEntry"
		LEFT JOIN TRIWALL_TRAINKEY."NNM1" AS A0 ON A0."Series"=A."Series";
	ELSE IF :DTYPE='GetBinLocation' THEN
		SELECT 
			 A."WhsCode"
			,A."BinCode"
		FROM TRIWALL_TRAINKEY."OBIN" AS A
		WHERE A."WhsCode"=:par1;
	ELSE IF :DTYPE='PriceList' THEN
		SELECT 
			 "ListNum" AS "Code"
			,"ListName" AS "Name" 
		FROM TRIWALL_TRAINKEY."OPLN";
	ELSE IF :DTYPE='GetPermission' THEN
		/*SELECT 
			* 
		FROM [BarCodeDatabase].[dbo].Permission*/
		SELECT 'Comming Soon' AS "TEST" FROM DUMMY;
	ELSE IF :DTYPE='FindPermission' THEN
		/*SELECT 
			A."Id" 
		FROM [BarCodeDatabase].[dbo].Permission A 
		LEFT JOIN [BarCodeDatabase].[dbo].PermissionUser B ON B.PermissionId = A.Id 
		WHERE B.UserCode =:Par1;*/
		SELECT 'Comming Soon' AS "TEST" FROM DUMMY;
	ELSE IF :DTYPE='GetVatCodePurchase' THEN
		SELECT 
			 "Code" AS "Code"
			,CAST("Rate" AS double) AS "Rate"
		FROM TRIWALL_TRAINKEY."OVTG"
		WHERE "Category"='I' AND "Inactive"='N';
	ELSE IF :DTYPE='GetTaxSale' THEN
		SELECT 
			 "Code" AS "Code"
			,CAST("Rate" AS double) AS "Rate"
		FROM TRIWALL_TRAINKEY."OVTG"
		WHERE "Category"='O' AND "Inactive"='N';
	ELSE IF :DTYPE='TotalItemCount' THEN
		IF :par1='GoodReceiptPo' THEN
			SELECT COUNT("CardCode") AS "AllItem" FROM TRIWALL_TRAINKEY."OPDN";
		ELSE IF :par1='PurchaseOrder' THEN
			SELECT COUNT("CardCode") AS "AllItem" FROM TRIWALL_TRAINKEY."OPOR" WHERE "DocStatus"='O';
		ELSE IF :par1='SaleOrder' THEN
			SELECT COUNT("CardCode") AS "AllItem" FROM TRIWALL_TRAINKEY."ODRF" WHERE "DocStatus"='O' AND "ObjType"='15';
		ELSE IF :par1='DeliveryOrder' THEN
			SELECT COUNT("CardCode") AS "AllItem" FROM TRIWALL_TRAINKEY."ODLN";
		ELSE IF :par1='IssueForProduction' THEN
			SELECT
				COUNT("DocNum") AS "AllItem" 
			FROM TRIWALL_TRAINKEY."OIGE" AS A
			--LEFT JOIN TRIWALL_TRAINKEY."IGE1" AS B ON B."DocEntry"=A."DocEntry" 
			WHERE (SELECT DISTINCT "BaseType" FROM TRIWALL_TRAINKEY."IGE1" WHERE "DocEntry"=A."DocEntry")='202';
		ELSE IF :par1='ReceiptFromProduction' THEN
			SELECT COUNT("DocNum") AS "AllItem" FROM TRIWALL_TRAINKEY."OIGN";
		ELSE IF :par1='InventoryTransfer' THEN
			SELECT COUNT("DocEntry") AS "AllItem" FROM TRIWALL_TRAINKEY."OWTR";
		ELSE IF :par1='SaleOrder' THEN
			SELECT COUNT("CardCode") AS "AllItem" FROM TRIWALL_TRAINKEY."ODRF" WHERE "DocStatus"='O' AND "ObjType"='15';
		ELSE IF :par1='DeliveryOrderReturn' THEN
			SELECT COUNT("CardCode") AS "AllItem" FROM TRIWALL_TRAINKEY."ODLN" WHERE "DocStatus"='O';
		ELSE IF :par1='Return' THEN
			SELECT COUNT("CardCode")  AS "AllItem" FROM TRIWALL_TRAINKEY."ORDN";
		ELSE IF :par1='GoodReturn' THEN
			SELECT COUNT("CardCode")  AS "AllItem" FROM TRIWALL_TRAINKEY."ORPD";
		ELSE IF :par1='GoodReceiptPOReturn' THEN
			SELECT COUNT("CardCode") AS "AllItem" FROM TRIWALL_TRAINKEY."OPDN" WHERE "DocStatus"='O';
		ELSE IF :par1='ARCreditMemo' THEN
			SELECT COUNT("CardCode")  AS "AllItem" FROM TRIWALL_TRAINKEY."ORIN";
		ELSE IF :par1='ARInvoiceOpenStatus' THEN
			SELECT COUNT("CardCode") AS "AllItem" FROM TRIWALL_TRAINKEY."OINV" WHERE "DocStatus"='O';
		ELSE IF :par1='InventoryCounting' THEN
			SELECT COUNT("DocNum") AS "AllItem" FROM TRIWALL_TRAINKEY."OINC";
		ELSE IF :par1='DeliveryOrderReturnRequest' THEN
			SELECT COUNT("CardCode") AS "AllItem" FROM TRIWALL_TRAINKEY."ODLN" WHERE "DocStatus"='O';
		ELSE IF :par1='ReturnRequest' THEN
			SELECT COUNT("CardCode")  AS "AllItem" FROM TRIWALL_TRAINKEY."ORRR";
		END IF;
		END IF;
		END IF;
		END IF;
		END IF;
		END IF;
		END IF;
		END IF;
		END IF;
		END IF;
		END IF;
		END IF;
		END IF;
		END IF;
		END IF;
		END IF;
		END IF;
	ELSE IF :DTYPE='GoodReceiptPoHeader' THEN
	
		IF :par2='' THEN
			DECLARE offset INT;
			SELECT CAST(:par1 AS INT)*10 INTO offset FROM DUMMY;
	
			SELECT 
				 "DocEntry" AS "DocEntry"
				,"DocNum" AS "DocumentNumber"
				,TO_VARCHAR("DocDate",'yyyy-MM-dd') AS "DocDate"
				,"CardCode" AS "VendorCode"
				,"Comments" AS "Remarks"
				,TO_VARCHAR("TaxDate",'yyyy-MM-dd') AS "TaxDate"
			FROM TRIWALL_TRAINKEY."OPDN" 
			ORDER BY "DocEntry" LIMIT 10 OFFSET :offset;
		ELSE IF :par2='condition' THEN
			SELECT 
				 "DocEntry" AS "DocEntry"
				,"DocNum" AS "DocumentNumber"
				,TO_VARCHAR("DocDate",'yyyy-MM-dd') AS "DocDate"
				,"CardCode" AS "VendorCode"
				,"Comments" AS "Remarks"
				,TO_VARCHAR("TaxDate",'yyyy-MM-dd') AS "TaxDate"
			FROM TRIWALL_TRAINKEY."OPDN" 
			WHERE "DocStatus"='O'
			AND "DocDate" BETWEEN CASE WHEN :par3='' THEN '1999-01-01' ELSE :par3 END AND CASE WHEN :par4='' THEN '2100-01-01' ELSE :par4 END
			AND "DocNum" LIKE CASE WHEN :par5='' OR :par5='0' THEN "DocNum" ELSE '%'||:par5||'%' END
			ORDER BY "DocEntry";
		END IF;
		END IF;
		
	ELSE IF :DTYPE='GET_PURCHASE_ORDER' THEN
		
		IF :par2='' THEN
			DECLARE offset INT;
			SELECT CAST(:par1 AS INT)*10 INTO offset FROM DUMMY;
			SELECT 
				 "DocEntry" AS "DocEntry"
				,"DocNum" AS "DocumentNumber"
				,TO_VARCHAR("DocDate",'yyyy-MM-dd') AS "DocDate"
				,"CardCode" AS "VendorCode"
				,"Comments" AS "Remarks"
				,TO_VARCHAR("TaxDate",'yyyy-MM-dd') AS "TaxDate"
			FROM TRIWALL_TRAINKEY."OPOR" 
			WHERE "DocStatus"='O'
			ORDER BY "DocEntry" LIMIT 10 OFFSET :offset;
		ELSE IF :par2='condition' THEN
			SELECT 
				 "DocEntry" AS "DocEntry"
				,"DocNum" AS "DocumentNumber"
				,TO_VARCHAR("DocDate",'yyyy-MM-dd') AS "DocDate"
				,"CardCode" AS "VendorCode"
				,"Comments" AS "Remarks"
				,TO_VARCHAR("TaxDate",'yyyy-MM-dd') AS "TaxDate"
			FROM TRIWALL_TRAINKEY."OPOR" 
			WHERE "DocStatus"='O'
			AND "DocDate" BETWEEN CASE WHEN :par3='' THEN '1999-01-01' ELSE :par3 END AND CASE WHEN :par4='' THEN '2100-01-01' ELSE :par4 END
			AND "DocNum" LIKE CASE WHEN :par5='' OR :par5='0' THEN "DocNum" ELSE '%'||:par5||'%' END
			ORDER BY "DocEntry";
		END IF;
		END IF;
	ELSE IF :DTYPE='IssueForProduction' THEN
	
		IF :par2='' THEN
			DECLARE offset INT;
			SELECT CAST(:par1 AS INT)*10 INTO offset FROM DUMMY;

			SELECT DISTINCT
				 A."DocEntry" AS "DocEntry"
				,A."DocNum" AS "DocumentNumber"
				,TO_VARCHAR(A."DocDate",'yyyy-MM-dd') AS "DocDate"
				,A."CardCode" AS "VendorCode"
				,A."Comments" AS "Remarks"
				,TO_VARCHAR(A."TaxDate",'yyyy-MM-dd') AS "TaxDate"
			FROM TRIWALL_TRAINKEY."OIGE" AS A
			LEFT JOIN TRIWALL_TRAINKEY."IGE1" AS B ON B."DocEntry"=A."DocEntry" 
			WHERE B."BaseType"='202'
			ORDER BY A."DocEntry" LIMIT 10 OFFSET :offset;
		ELSE IF :par2='condition' THEN
			SELECT DISTINCT
				 A."DocEntry" AS "DocEntry"
				,A."DocNum" AS "DocumentNumber"
				,TO_VARCHAR(A."DocDate",'yyyy-MM-dd') AS "DocDate"
				,A."CardCode" AS "VendorCode"
				,A."Comments" AS "Remarks"
				,TO_VARCHAR(A."TaxDate",'yyyy-MM-dd') AS "TaxDate"
			FROM TRIWALL_TRAINKEY."OIGE" AS A
			LEFT JOIN TRIWALL_TRAINKEY."IGE1" AS B ON B."DocEntry"=A."DocEntry" 
			WHERE A."DocStatus"='O'
			AND B."BaseType"='202'
			AND A."DocDate" BETWEEN CASE WHEN :par3='' THEN '1999-01-01' ELSE :par3 END AND CASE WHEN :par4='' THEN '2100-01-01' ELSE :par4 END
			AND A."DocNum" LIKE CASE WHEN :par5='' OR :par5='0' THEN "DocNum" ELSE '%'||:par5||'%' END
			ORDER BY "DocEntry";
		END IF;
		END IF;
		
	ELSE IF :DTYPE='GetDeliveryOrderHeader' THEN

		IF :par2='' THEN
			DECLARE offset INT;
			SELECT CAST(:par1 AS INT)*10 INTO offset FROM DUMMY;
			SELECT 
				 "DocEntry" AS "DocEntry"
				,"DocNum" AS "DocumentNumber"
				,TO_VARCHAR("DocDate",'yyyy-MM-dd') AS "DocDate"
				,"CardCode" AS "VendorCode"
				,"Comments" AS "Remarks"
				,TO_VARCHAR("TaxDate",'yyyy-MM-dd') AS "TaxDate"
			FROM TRIWALL_TRAINKEY."ODLN" 
			ORDER BY "DocEntry" LIMIT 10 OFFSET :offset;
		ELSE IF :par2='condition' THEN
			SELECT 
				 "DocEntry" AS "DocEntry"
				,"DocNum" AS "DocumentNumber"
				,TO_VARCHAR("DocDate",'yyyy-MM-dd') AS "DocDate"
				,"CardCode" AS "VendorCode"
				,"Comments" AS "Remarks"
				,TO_VARCHAR("TaxDate",'yyyy-MM-dd') AS "TaxDate"
			FROM TRIWALL_TRAINKEY."ODLN" 
			WHERE "DocStatus"='O'
			AND "DocDate" BETWEEN CASE WHEN :par3='' THEN '1999-01-01' ELSE :par3 END AND CASE WHEN :par4='' THEN '2100-01-01' ELSE :par4 END
			AND "DocNum" LIKE CASE WHEN :par5='' OR :par5='0' THEN "DocNum" ELSE '%'||:par5||'%' END
			ORDER BY "DocEntry";
		END IF;
		END IF;
	ELSE IF :DTYPE='GetSaleOrder' THEN
		IF :par2='' THEN
			DECLARE offset INT;
			SELECT CAST(:par1 AS INT)*10 INTO offset FROM DUMMY;
			SELECT 
				 "DocEntry" AS "DocEntry"
				,"DocNum" AS "DocumentNumber"
				,TO_VARCHAR("DocDate",'yyyy-MM-dd') AS "DocDate"
				,"CardCode" AS "VendorCode"
				,"Comments" AS "Remarks"
				,TO_VARCHAR("TaxDate",'yyyy-MM-dd') AS "TaxDate"
			FROM TRIWALL_TRAINKEY."ODRF" 
			WHERE "DocStatus"='O' AND "ObjType"='15'
			ORDER BY "DocEntry" LIMIT 10 OFFSET :offset;
		ELSE IF :par2='condition' THEN
			SELECT 
				 "DocEntry" AS "DocEntry"
				,"DocNum" AS "DocumentNumber"
				,TO_VARCHAR("DocDate",'yyyy-MM-dd') AS "DocDate"
				,"CardCode" AS "VendorCode"
				,"Comments" AS "Remarks"
				,TO_VARCHAR("TaxDate",'yyyy-MM-dd') AS "TaxDate"
			FROM TRIWALL_TRAINKEY."ODRF" 
			WHERE "DocStatus"='O' AND "ObjType"='15' AND
			 	"DocDate" BETWEEN CASE WHEN :par3='' THEN '1999-01-01' ELSE :par3 END AND CASE WHEN :par4='' THEN '2100-01-01' ELSE :par4 END
			AND "DocNum" LIKE CASE WHEN :par5='' OR :par5='0' THEN "DocNum" ELSE '%'||:par5||'%' END
			ORDER BY "DocEntry";
		END IF;
		END IF;
	ELSE IF :DTYPE='GET_PurchaseOrder_Line_Detail_By_DocNum' THEN
	
		SELECT 
			 B."LineNum" AS "BaseLineNumber"
			,B."DocEntry" AS "DocEntry"
			,B."ItemCode" AS "ItemCode"
			,B."Dscription" AS "ItemName"
			,B."OpenQty" AS "Qty"
			,B."Price" AS "Price"
			,B."LineTotal" AS "LineTotal"
			,B."VatGroup" AS "VatCode"
			,B."WhsCode" AS "WarehouseCode"
			,E."CodeBars" AS "BarCode"
			,CASE WHEN E."ManBtchNum"='Y' THEN
				'B'
			 WHEN E."ManSerNum"='Y' THEN
				'S'
			 ELSE
				'N'
			 END AS "ManageItem"
		FROM TRIWALL_TRAINKEY."OPOR" AS A
		LEFT JOIN TRIWALL_TRAINKEY."POR1" AS B ON A."DocEntry"=B."DocEntry"
		LEFT JOIN TRIWALL_TRAINKEY."OCPR" AS C ON A."CardCode"=C."CardCode" AND A."CntctCode"=C."CntctCode"
		LEFT JOIN TRIWALL_TRAINKEY."OITM" AS E ON E."ItemCode"=B."ItemCode"
		WHERE A."DocEntry"=:par1 AND B."LineStatus"='O';
		
	ELSE IF :DTYPE='GetCustomer' THEN
		SELECT  
			 "CardCode" AS "VendorCode"
			,"CardName" As "VendorName"
			,"Phone1" As "PhoneNumber"
			,"CntctPrsn" as "ContactID"
		FROM TRIWALL_TRAINKEY."OCRD" 
		WHERE "CardType"='C';
	ELSE IF :DTYPE='GetDeliveryOrderLineDetailByDocEntry' THEN
		SELECT 
			 B."LineNum" AS "BaseLineNumber"
			,B."DocEntry" AS "DocEntry"
			,B."ItemCode" AS "ItemCode"
			,B."Dscription" AS "ItemName"
			,B."OpenQty" AS "Qty"
			,B."Price" AS "Price"
			,B."LineTotal" AS "LineTotal"
			,B."TaxCode" AS "VatCode"
			,B."WhsCode" AS "WarehouseCode"
			,E."CodeBars" AS "BarCode"
			,CASE WHEN E."ManBtchNum"='Y' THEN
				'B'
			 WHEN E."ManSerNum"='Y' THEN
				'S'
			 ELSE
				'N'
			 END AS "ManageItem"
		FROM TRIWALL_TRAINKEY."ODLN" AS A
		LEFT JOIN TRIWALL_TRAINKEY."DLN1" AS B ON A."DocEntry"=B."DocEntry"
		LEFT JOIN TRIWALL_TRAINKEY."OCPR" AS C ON A."CardCode"=C."CardCode" AND A."CntctCode"=C."CntctCode"
		LEFT JOIN TRIWALL_TRAINKEY."OITM" AS E ON E."ItemCode"=B."ItemCode"
		WHERE A."DocEntry"=:par1;
	ELSE IF :DTYPE='GetSaleOrderLineDetailByDocEntry' THEN
	
	
		SELECT 
			 B."LineNum" AS "BaseLineNumber"
			,B."DocEntry" AS "DocEntry"
			,B."ItemCode" AS "ItemCode"
			,B."Dscription" AS "ItemName"
			--,B."OpenQty" AS "Qty"
			,B."Quantity" AS "Qty"
			,B."Price" AS "Price"
			,B."LineTotal" AS "LineTotal"
			,IFNULL(B."VatGroup",'S00') AS "VatCode"
			,B."WhsCode" AS "WarehouseCode"
			,E."CodeBars" AS "BarCode"
			,CASE WHEN E."ManBtchNum"='Y' THEN
				'B'
			 WHEN E."ManSerNum"='Y' THEN
				'S'
			 ELSE
				'N'
			 END AS "ManageItem"
		FROM TRIWALL_TRAINKEY."ODRF" AS A
		LEFT JOIN TRIWALL_TRAINKEY."DRF1" AS B ON A."DocEntry"=B."DocEntry"
		LEFT JOIN TRIWALL_TRAINKEY."OCPR" AS C ON A."CardCode"=C."CardCode" AND A."CntctCode"=C."CntctCode"
		LEFT JOIN TRIWALL_TRAINKEY."OITM" AS E ON E."ItemCode"=B."ItemCode"
		WHERE A."DocEntry"=:par1 AND A."ObjType"='15' AND B."LineStatus"='O';
			
	ELSE IF :DTYPE='GET_DeliveryOrder_Header_Detail_By_DocNum' THEN
		SELECT 
			 F."SeriesName" AS "SeriesName"
			,A."DocNum" AS "DocNum"
			,TO_VARCHAR(A."DocDate",'yyyy-mm-dd') AS "DocDate"
			,TO_VARCHAR(A."TaxDate",'yyyy-mm-dd') AS "TaxDate"
			,A."CardCode"|| ' - ' || A."CardName" AS "Vendor"
			,IFNULL(C."Name",'') AS "ContactPerson"
			,IFNULL(A."NumAtCard",'') AS "RefInv"
		FROM TRIWALL_TRAINKEY."ODLN" AS A
		LEFT JOIN TRIWALL_TRAINKEY."OCPR" AS C ON A."CardCode"=C."CardCode" AND A."CntctCode"=C."CntctCode"
		LEFT JOIN TRIWALL_TRAINKEY."NNM1" AS F ON F."Series"=A."Series"
		WHERE 
			A."DocEntry"=:par1;
	ELSE IF :DTYPE='GET_SaleOrder_Header_Detail_By_DocNum' THEN
		SELECT 
			 F."SeriesName" AS "SeriesName"
			,A."DocNum" AS "DocNum"
			,TO_VARCHAR(A."DocDate",'yyyy-mm-dd') AS "DocDate"
			,TO_VARCHAR(A."TaxDate",'yyyy-mm-dd') AS "TaxDate"
			,A."CardCode" AS "Vendor"
			,IFNULL(C."Name",'') AS "ContactPerson"
			,IFNULL(A."NumAtCard",'') AS "RefInv"
		FROM TRIWALL_TRAINKEY."ODRF" AS A
		LEFT JOIN TRIWALL_TRAINKEY."OCPR" AS C ON A."CardCode"=C."CardCode" AND A."CntctCode"=C."CntctCode"
		LEFT JOIN TRIWALL_TRAINKEY."NNM1" AS F ON F."Series"=A."Series"
		WHERE 
			A."DocEntry"=:par1 AND A."ObjType"='15';
	ELSE IF :DTYPE='OnGetBatchOrSerialAvailableByItemCode' THEN		
		IF :par1='S' THEN
			SELECT 
				 B."ItemCode" AS "ItemCode"	
				,B."Quantity" AS "Qty"
				,B."DistNumber" AS "SerialBatch"
				,B."MnfSerial" AS "MfrSerialNo"
				,TO_VARCHAR(B."ExpDate",'yyyy-MM-dd') AS "ExpDate"
				,TO_VARCHAR(B."MnfDate",'yyyy-MM-dd') AS "MrfDate"
				,'Serial' AS "Type"
			FROM TRIWALL_TRAINKEY."OSRN" AS B
			LEFT JOIN TRIWALL_TRAINKEY."OSRI" AS C On C."ItemCode"=B."ItemCode" And C."SysSerial"=B."SysNumber"
			WHERE B."ItemCode"=:par2 AND C."Status"='0';
		ELSE IF :par1='B' THEN
			SELECT 
				 B."ItemCode" AS "ItemCode"	
				,C."Quantity" AS "Qty"
				,B."DistNumber" AS "SerialBatch"
				,B."MnfSerial" AS "MfrSerialNo"
				,TO_VARCHAR("ExpDate",'yyyy-MM-dd') AS "ExpDate"
				,TO_VARCHAR(B."MnfDate",'yyyy-MM-dd') AS "MrfDate"
				,'Batch' AS "Type"
			FROM TRIWALL_TRAINKEY."OBTN" AS B
			LEFT JOIN TRIWALL_TRAINKEY."OBTQ" AS C ON C."ItemCode"=B."ItemCode" and B."SysNumber"=C."SysNumber"
			WHERE B."ItemCode"=:par2 AND IFNULL(C."Quantity",0)>0;
		END IF;
		END IF;
	ELSE IF :DTYPE='OnGetBatchOrSerialInIssueForProduction' THEN		
		IF :par1='S' THEN
			SELECT 
				 B."ItemCode" AS "ItemCode"	
				,B."Quantity" AS "Qty"
				,B."DistNumber" AS "SerialBatch"
				,B."MnfSerial" AS "MfrSerialNo"
				,B."ExpDate" AS "ExpDate"
				,B."MnfDate" AS "MrfDate"
				,'Serial' AS "Type"
			FROM TRIWALL_TRAINKEY."OSRN" AS B
			LEFT JOIN TRIWALL_TRAINKEY."OSRI" AS C On C."ItemCode"=B."ItemCode" And C."SysSerial"=B."SysNumber"
			LEFT JOIN TRIWALL_TRAINKEY."SRI1" AS D ON D."SysSerial"=B."SysNumber"
			WHERE B."ItemCode"=:par2
				AND D."BsDocType"='202'
				AND D."BaseType"='60' 
				AND D."BsDocEntry"=:par3 
				AND C."Status"<>'0';
		ELSE IF :par1='B' THEN
			SELECT 
				 B."ItemCode" AS "ItemCode"	
				,B."Quantity" AS "Qty"
				,B."BatchNum" AS "SerialBatch"
				,C."MnfSerial" AS "MfrSerialNo"
				,C."ExpDate" AS "ExpDate"
				,C."MnfDate" AS "MrfDate"
				,'Batch' AS "Type"
			FROM TRIWALL_TRAINKEY."IBT1" AS B
			LEFT JOIN TRIWALL_TRAINKEY."OBTN" AS C ON C."ItemCode"=B."ItemCode" and C."DistNumber"=B."BatchNum"
			WHERE 
				B."ItemCode"=:par2 
				AND B."BsDocType"='202'
				AND B."BaseType"='60' 
				AND B."BsDocEntry"=:par3;
				--AND B."BsDocEntry"='723';
				/*AND B."Quantity"<(SELECT T0."Quantity"
									FROM TRIWALL_TRAINKEY."IBT1" AS T0 
									WHERE 
										T0."ItemCode"=B."ItemCode" 
									AND T0."BaseType"='59' 
									AND T0."BsDocType"='202' 
									AND B."BsDocEntry"='723')*/
		END IF;
		END IF;
	ELSE IF :DTYPE='GET_Production_Order_Lines' THEN
		/*
			execute immediate '
			SELECT  
			 A."DocEntry" AS "DocEntry"
			,A."LineNum" AS "OrderLineNum"
			,A."ItemCode" AS "ItemCode"
			,A."ItemName" AS "ItemName"
			,A."PlannedQty" AS "Qty"
			,A."UomCode" AS "Uom"
			,A."wareHouse" AS "WarehouseCode" 
			,CASE WHEN B."ManSerNum"=''Y'' THEN
			 	''S''
			 WHEN B."ManBtchNum"=''Y'' THEN
			 	''B''
			 ELSE ''N'' END AS "ItemType"
			FROM TRIWALL_TRAINKEY."WOR1" AS A
			LEFT JOIN TRIWALL_TRAINKEY."OWOR" AS AA ON AA."DocEntry"=A."DocEntry"
			LEFT JOIN TRIWALL_TRAINKEY."OITM" AS B ON B."ItemCode"=A."ItemCode"
			WHERE A."DocEntry" IN ('|| '841' ||')  AND A."IssuedQty"=0;'; --AND A."IssueType"=''M''
		*/
		execute immediate '
			SELECT  
			 A."DocEntry" AS "DocEntry"
			,A."LineNum" AS "OrderLineNum"
			,A."ItemCode" AS "ItemCode"
			,A."ItemName" AS "ItemName"
			,A."PlannedQty" AS "Qty"
			,A."UomCode" AS "Uom"
			,A."wareHouse" AS "WarehouseCode" 
			,CASE WHEN B."ManSerNum"=''Y'' THEN
			 	''S''
			 WHEN B."ManBtchNum"=''Y'' THEN
			 	''B''
			 ELSE ''N'' END AS "ItemType"
			FROM TRIWALL_TRAINKEY."WOR1" AS A
			LEFT JOIN TRIWALL_TRAINKEY."OWOR" AS AA ON AA."DocEntry"=A."DocEntry"
			LEFT JOIN TRIWALL_TRAINKEY."OITM" AS B ON B."ItemCode"=A."ItemCode"
			WHERE A."DocEntry" IN ('|| :par1 ||') AND A."IssueType"=''M'' AND A."IssuedQty"=0;';
	ELSE IF :DTYPE='GET_IssueForProduction_Header_Detail_By_DocNum' THEN
		SELECT 
			 F."SeriesName" AS "SeriesName"
			,A."DocNum" AS "DocNum"
			,TO_VARCHAR(A."DocDate",'yyyy-mm-dd') AS "DocDate"
			,TO_VARCHAR(A."TaxDate",'yyyy-mm-dd') AS "TaxDate"
			,A."CardCode"|| ' - ' || A."CardName" AS "Vendor"
			,IFNULL(C."Name",'') AS "ContactPerson"
			,IFNULL(A."NumAtCard",'') AS "RefInv"
		FROM TRIWALL_TRAINKEY."OIGE" AS A
		LEFT JOIN TRIWALL_TRAINKEY."OCPR" AS C ON A."CardCode"=C."CardCode" AND A."CntctCode"=C."CntctCode"
		LEFT JOIN TRIWALL_TRAINKEY."NNM1" AS F ON F."Series"=A."Series"
		WHERE 
			A."DocEntry"=:par1;
	ELSE IF :DTYPE='GetIssueForProductionLineDetailByDocEntry' THEN
		SELECT 
			 B."LineNum" AS "BaseLineNumber"
			,B."DocEntry" AS "DocEntry"
			,B."ItemCode" AS "ItemCode"
			,B."Dscription" AS "ItemName"
			,B."OpenQty" AS "Qty"
			,B."Price" AS "Price"
			,B."LineTotal" AS "LineTotal"
			,B."TaxCode" AS "VatCode"
			,B."WhsCode" AS "WarehouseCode"
			,E."CodeBars" AS "BarCode"
			,CASE WHEN E."ManBtchNum"='Y' THEN
				'B'
			 WHEN E."ManSerNum"='Y' THEN
				'S'
			 ELSE
				'N'
			 END AS "ManageItem"
		FROM TRIWALL_TRAINKEY."OIGE" AS A
		LEFT JOIN TRIWALL_TRAINKEY."IGE1" AS B ON A."DocEntry"=B."DocEntry"
		LEFT JOIN TRIWALL_TRAINKEY."OCPR" AS C ON A."CardCode"=C."CardCode" AND A."CntctCode"=C."CntctCode"
		LEFT JOIN TRIWALL_TRAINKEY."OITM" AS E ON E."ItemCode"=B."ItemCode"
		WHERE A."DocEntry"=:par1;
	ELSE IF :DTYPE='GetBatchSerialIssueForProduction' THEN
		SELECT ROW_NUMBER() OVER(ORDER BY "ItemCode") AS "LineNum",* FROM (
			SELECT 
				 A."ItemCode" AS "ItemCode"	
				,1 AS "Qty"
				,B."DistNumber" AS "SerialBatch"
				,B."MnfSerial" AS "MfrSerialNo"
				,TO_VARCHAR(B."ExpDate",'dd-MM-yyyy') AS "ExpDate"
				,B."MnfDate" AS "MrfDate"
				,'Serial' AS "Type"
			FROM TRIWALL_TRAINKEY."SRI1" AS A
			LEFT JOIN TRIWALL_TRAINKEY."OSRN" AS B ON A."ItemCode"=B."ItemCode" AND B."SysNumber"=A."SysSerial"
			LEFT JOIN TRIWALL_TRAINKEY."OSRI" AS C On C."ItemCode"=A."ItemCode" And C."SysSerial"=A."SysSerial"
			WHERE A."BaseEntry"=:par1 
				AND A."BaseType"=60 
				--And C."Status"<>0
					
			UNION ALL
			
			SELECT 
				 A."ItemCode" AS "ItemCode"	
				,A."Quantity" AS "Qty"
				,B."DistNumber" AS "SerialBatch"
				,B."MnfSerial" AS "MfrSerialNo"
				,TO_VARCHAR("ExpDate",'dd-MM-yyyy') AS "ExpDate"
				,B."MnfDate" AS "MrfDate"
				,'Batch' AS "Type"
			FROM TRIWALL_TRAINKEY."IBT1" AS A 
			LEFT JOIN TRIWALL_TRAINKEY."OBTN" AS B ON A."ItemCode"=B."ItemCode" AND A."BatchNum"=B."DistNumber"
			--LEFT JOIN TRIWALL_TRAINKEY."OBTQ" AS C ON C."ItemCode"=A."ItemCode" and B."SysNumber"=C."SysNumber"
			WHERE A."BaseEntry"=:par1
				AND A."BaseType"=60
		)AS A;
	ELSE IF :DTYPE='GET_Issue_Production_Lines' THEN
		execute immediate '
			SELECT  
				-- A."DocEntry" AS "DocEntry"
				 A."BaseEntry" AS "DocEntry"
				--,A."LineNum" AS "OrderLineNum"
				,A."BaseLine" AS "OrderLineNum"
				,A."ItemCode" AS "ItemCode"
				,B."ItemName" AS "ItemName"
				,A."Quantity" AS "Qty"
				,C."PlannedQty" AS "PlanQty"
				,A."UomCode" AS "Uom"
				,A."WhsCode" AS "WarehouseCode" 
				,CASE WHEN B."ManSerNum"=''Y'' THEN
				 	''S''
				 WHEN B."ManBtchNum"=''Y'' THEN
				 	''B''
				 ELSE ''N'' END AS "ItemType"
			FROM TRIWALL_TRAINKEY."IGE1" AS A
			LEFT JOIN TRIWALL_TRAINKEY."OITM" AS B ON B."ItemCode"=A."ItemCode"
			LEFT JOIN TRIWALL_TRAINKEY."WOR1" AS C ON C."ItemCode"=A."ItemCode" AND C."DocEntry"=A."BaseEntry" AND C."LineNum"=A."BaseLine"
			WHERE A."BaseEntry" IN ('|| :par1 ||') AND A."BaseType"=''202'';';
	ELSE IF :DTYPE='ReceiptForProduction' THEN
	
		IF :par2='' THEN
			DECLARE offset INT;
			SELECT CAST(:par1 AS INT)*10 INTO offset FROM DUMMY;

			SELECT DISTINCT
				 A."DocEntry" AS "DocEntry"
				,A."DocNum" AS "DocumentNumber"
				,TO_VARCHAR(A."DocDate",'yyyy-MM-dd') AS "DocDate"
				,A."CardCode" AS "VendorCode"
				,A."Comments" AS "Remarks"
				,TO_VARCHAR(A."TaxDate",'yyyy-MM-dd') AS "TaxDate"
			FROM TRIWALL_TRAINKEY."OIGN" AS A
			LEFT JOIN TRIWALL_TRAINKEY."IGN1" AS B ON B."DocEntry"=A."DocEntry" 
			WHERE B."BaseType"='202'
			ORDER BY A."DocEntry" LIMIT 10 OFFSET :offset;
		ELSE IF :par2='condition' THEN
			SELECT DISTINCT
				 A."DocEntry" AS "DocEntry"
				,A."DocNum" AS "DocumentNumber"
				,TO_VARCHAR(A."DocDate",'yyyy-MM-dd') AS "DocDate"
				,A."CardCode" AS "VendorCode"
				,A."Comments" AS "Remarks"
				,TO_VARCHAR(A."TaxDate",'yyyy-MM-dd') AS "TaxDate"
			FROM TRIWALL_TRAINKEY."OIGN" AS A
			LEFT JOIN TRIWALL_TRAINKEY."IGN1" AS B ON B."DocEntry"=A."DocEntry" 
			WHERE A."DocStatus"='O'
			AND B."BaseType"='202'
			AND A."DocDate" BETWEEN CASE WHEN :par3='' THEN '1999-01-01' ELSE :par3 END AND CASE WHEN :par4='' THEN '2100-01-01' ELSE :par4 END
			AND A."DocNum" LIKE CASE WHEN :par5='' OR :par5='0' THEN "DocNum" ELSE '%'||:par5||'%' END
			ORDER BY "DocEntry";
		END IF;
		END IF;
	ELSE IF :DTYPE='GET_ReceiptForProduction_Header_Detail_By_DocNum' THEN
		SELECT 
			 F."SeriesName" AS "SeriesName"
			,A."DocNum" AS "DocNum"
			,TO_VARCHAR(A."DocDate",'yyyy-mm-dd') AS "DocDate"
			,TO_VARCHAR(A."TaxDate",'yyyy-mm-dd') AS "TaxDate"
			,A."CardCode"|| ' - ' || A."CardName" AS "Vendor"
			,IFNULL(C."Name",'') AS "ContactPerson"
			,IFNULL(A."NumAtCard",'') AS "RefInv"
			,A."DocEntry" AS "DocEntry"
		FROM TRIWALL_TRAINKEY."OIGN" AS A
		LEFT JOIN TRIWALL_TRAINKEY."OCPR" AS C ON A."CardCode"=C."CardCode" AND A."CntctCode"=C."CntctCode"
		LEFT JOIN TRIWALL_TRAINKEY."NNM1" AS F ON F."Series"=A."Series"
		WHERE 
			A."DocEntry"=:par1;
	ELSE IF :DTYPE='GetReceiptForProductionLineDetailByDocEntry' THEN
		SELECT 
			 B."LineNum" AS "BaseLineNumber"
			,B."DocEntry" AS "DocEntry"
			,B."ItemCode" AS "ItemCode"
			,B."Dscription" AS "ItemName"
			,B."OpenQty" AS "Qty"
			,B."Price" AS "Price"
			,B."LineTotal" AS "LineTotal"
			,B."TaxCode" AS "VatCode"
			,B."WhsCode" AS "WarehouseCode"
			,E."CodeBars" AS "BarCode"
			,CASE WHEN E."ManBtchNum"='Y' THEN
				'B'
			 WHEN E."ManSerNum"='Y' THEN
				'S'
			 ELSE
				'N'
			 END AS "ManageItem"
		FROM TRIWALL_TRAINKEY."OIGN" AS A
		LEFT JOIN TRIWALL_TRAINKEY."IGN1" AS B ON A."DocEntry"=B."DocEntry"
		LEFT JOIN TRIWALL_TRAINKEY."OCPR" AS C ON A."CardCode"=C."CardCode" AND A."CntctCode"=C."CntctCode"
		LEFT JOIN TRIWALL_TRAINKEY."OITM" AS E ON E."ItemCode"=B."ItemCode"
		WHERE A."DocEntry"=:par1;
	ELSE IF :DTYPE='GetBatchSerialReceiptForProduction' THEN
		SELECT ROW_NUMBER() OVER(ORDER BY "ItemCode") AS "LineNum",* FROM (
			SELECT 
				 A."ItemCode" AS "ItemCode"	
				,1 AS "Qty"
				,B."DistNumber" AS "SerialBatch"
				,B."MnfSerial" AS "MfrSerialNo"
				,TO_VARCHAR(B."ExpDate",'dd-MM-yyyy') AS "ExpDate"
				,B."MnfDate" AS "MrfDate"
				,'Serial' AS "Type"
			FROM TRIWALL_TRAINKEY."SRI1" AS A
			LEFT JOIN TRIWALL_TRAINKEY."OSRN" AS B ON A."ItemCode"=B."ItemCode" AND B."SysNumber"=A."SysSerial"
			LEFT JOIN TRIWALL_TRAINKEY."OSRI" AS C On C."ItemCode"=A."ItemCode" And C."SysSerial"=A."SysSerial"
			WHERE A."BaseEntry"=:par1 
				AND A."BaseType"=59
				--And C."Status"<>0
					
			UNION ALL
			
			SELECT 
				 A."ItemCode" AS "ItemCode"	
				,A."Quantity" AS "Qty"
				,B."DistNumber" AS "SerialBatch"
				,B."MnfSerial" AS "MfrSerialNo"
				,TO_VARCHAR("ExpDate",'dd-MM-yyyy') AS "ExpDate"
				,B."MnfDate" AS "MrfDate"
				,'Batch' AS "Type"
			FROM TRIWALL_TRAINKEY."IBT1" AS A 
			LEFT JOIN TRIWALL_TRAINKEY."OBTN" AS B ON A."ItemCode"=B."ItemCode" AND A."BatchNum"=B."DistNumber"
			--LEFT JOIN TRIWALL_TRAINKEY."OBTQ" AS C ON C."ItemCode"=A."ItemCode" and B."SysNumber"=C."SysNumber"
			WHERE A."BaseEntry"=:par1
				AND A."BaseType"=59
		)AS A;
	ELSE IF :DTYPE='GetReturnHeader' THEN
		IF :par2='' THEN
			DECLARE offset INT;
			SELECT CAST(:par1 AS INT)*10 INTO offset FROM DUMMY;
			SELECT 
				 "DocEntry" AS "DocEntry"
				,"DocNum" AS "DocumentNumber"
				,TO_VARCHAR("DocDate",'yyyy-MM-dd') AS "DocDate"
				,"CardCode" AS "VendorCode"
				,"Comments" AS "Remarks"
				,TO_VARCHAR("TaxDate",'yyyy-MM-dd') AS "TaxDate"
			FROM TRIWALL_TRAINKEY."ORDN" 
			ORDER BY "DocEntry" LIMIT 10 OFFSET :offset;
		ELSE IF :par2='condition' THEN
			SELECT 
				 "DocEntry" AS "DocEntry"
				,"DocNum" AS "DocumentNumber"
				,TO_VARCHAR("DocDate",'yyyy-MM-dd') AS "DocDate"
				,"CardCode" AS "VendorCode"
				,"Comments" AS "Remarks"
				,TO_VARCHAR("TaxDate",'yyyy-MM-dd') AS "TaxDate"
			FROM TRIWALL_TRAINKEY."ORDN" 
			WHERE "DocStatus"='O'
			AND "DocDate" BETWEEN :par3 AND :par4
			AND "DocNum" LIKE CASE WHEN :par5='' OR :par5='0' THEN "DocNum" ELSE '%'||:par5||'%' END
			ORDER BY "DocEntry";
		END IF;
		END IF;
	ELSE IF :DTYPE='GetGoodReturnHeader' THEN
		IF :par2='' THEN
			DECLARE offset INT;
			SELECT CAST(:par1 AS INT)*10 INTO offset FROM DUMMY;
			SELECT 
				 "DocEntry" AS "DocEntry"
				,"DocNum" AS "DocumentNumber"
				,TO_VARCHAR("DocDate",'yyyy-MM-dd') AS "DocDate"
				,"CardCode" AS "VendorCode"
				,"Comments" AS "Remarks"
				,TO_VARCHAR("TaxDate",'yyyy-MM-dd') AS "TaxDate"
			FROM TRIWALL_TRAINKEY."ORPD" 
			ORDER BY "DocEntry" LIMIT 10 OFFSET :offset;
		ELSE IF :par2='condition' THEN
			SELECT 
				 "DocEntry" AS "DocEntry"
				,"DocNum" AS "DocumentNumber"
				,TO_VARCHAR("DocDate",'yyyy-MM-dd') AS "DocDate"
				,"CardCode" AS "VendorCode"
				,"Comments" AS "Remarks"
				,TO_VARCHAR("TaxDate",'yyyy-MM-dd') AS "TaxDate"
			FROM TRIWALL_TRAINKEY."ORPD" 
			WHERE --"DocStatus"='O' AND
			 	"DocDate" BETWEEN CASE WHEN :par3='' THEN '1999-01-01' ELSE :par3 END AND CASE WHEN :par4='' THEN '2100-01-01' ELSE :par4 END
			AND "DocNum" LIKE CASE WHEN :par5='' OR :par5='0' THEN "DocNum" ELSE '%'||:par5||'%' END
			ORDER BY "DocEntry";
		END IF;
		END IF;
	ELSE IF :DTYPE='OnGetBatchOrSerialByItemCodeReuturnDelivery' THEN		
		IF :par1='S' THEN
			SELECT 
				 B."ItemCode" AS "ItemCode"	
				,1 AS "Qty"
				,B."DistNumber" AS "SerialBatch"
				,B."MnfSerial" AS "MfrSerialNo"
				,B."ExpDate" AS "ExpDate"
				,B."MnfDate" AS "MrfDate"
				,'Serial' AS "Type"
			FROM TRIWALL_TRAINKEY."SRI1" AS A
			LEFT JOIN TRIWALL_TRAINKEY."OSRN" AS B On A."ItemCode"=B."ItemCode" And A."SysSerial"=B."SysNumber"
			WHERE 
					A."ItemCode"=:par2 
				AND A."BaseEntry"=:par3
				AND A."BaseType"='15';
		ELSE IF :par1='B' THEN
			SELECT 
				 B."ItemCode" AS "ItemCode"	
				,B."Quantity" AS "Qty"
				,B."BatchNum" AS "SerialBatch"
				,C."MnfSerial" AS "MfrSerialNo"
				,C."ExpDate" AS "ExpDate"
				,C."MnfDate" AS "MrfDate"
				,'Batch' AS "Type"
			FROM TRIWALL_TRAINKEY."IBT1" AS B
			LEFT JOIN TRIWALL_TRAINKEY."OBTN" AS C ON C."ItemCode"=B."ItemCode" and C."DistNumber"=B."BatchNum"
			WHERE 
					B."ItemCode"=:par2 
				AND B."BaseEntry"=:par3
				AND B."BaseType"='15';
		END IF;
		END IF;
	ELSE IF :DTYPE='GetDeliveryOrderReturn' THEN

		DECLARE offset INT;
		SELECT CAST(:par1 AS INT)*10 INTO offset FROM DUMMY;

		SELECT 
			 "DocEntry" AS "DocEntry"
			,"DocNum" AS "DocumentNumber"
			,"DocDate" AS "DocDate"
			,"CardCode" AS "VendorCode"
			,"Comments" AS "Remarks"
			,"TaxDate" AS "TaxDate"
		FROM TRIWALL_TRAINKEY."ODLN" 
		WHERE "DocStatus"='O'
		ORDER BY "DocEntry" LIMIT 10 OFFSET :offset;
		
	ELSE IF :DTYPE='GetGoodReceiptPOReturn' THEN

		DECLARE offset INT;
		SELECT CAST(:par1 AS INT)*10 INTO offset FROM DUMMY;

		SELECT 
			 "DocEntry" AS "DocEntry"
			,"DocNum" AS "DocumentNumber"
			,"DocDate" AS "DocDate"
			,"CardCode" AS "VendorCode"
			,"Comments" AS "Remarks"
			,"TaxDate" AS "TaxDate"
		FROM TRIWALL_TRAINKEY."OPDN" 
		WHERE "DocStatus"='O'
		ORDER BY "DocEntry" LIMIT 10 OFFSET :offset;
	
	ELSE IF :DTYPE='GET_Return_Header_Detail_By_DocNum' THEN
		SELECT 
			 F."SeriesName" AS "SeriesName"
			,A."DocNum" AS "DocNum"
			,TO_VARCHAR(A."DocDate",'yyyy-mm-dd') AS "DocDate"
			,TO_VARCHAR(A."TaxDate",'yyyy-mm-dd') AS "TaxDate"
			,A."CardCode"|| ' - ' || A."CardName" AS "Vendor"
			,IFNULL(C."Name",'') AS "ContactPerson"
			,IFNULL(A."NumAtCard",'') AS "RefInv"
		FROM TRIWALL_TRAINKEY."ORDN" AS A
		LEFT JOIN TRIWALL_TRAINKEY."OCPR" AS C ON A."CardCode"=C."CardCode" AND A."CntctCode"=C."CntctCode"
		LEFT JOIN TRIWALL_TRAINKEY."NNM1" AS F ON F."Series"=A."Series"
		WHERE 
			A."DocEntry"=:par1;
	ELSE IF :DTYPE='GET_Delivery_Order_Header_Detail_By_DocNum_Return' THEN
		SELECT 
			 F."SeriesName" AS "SeriesName"
			,A."DocNum" AS "DocNum"
			,TO_VARCHAR(A."DocDate",'yyyy-mm-dd') AS "DocDate"
			,TO_VARCHAR(A."TaxDate",'yyyy-mm-dd') AS "TaxDate"
			,A."CardCode" AS "Vendor"
			,IFNULL(C."Name",'') AS "ContactPerson"
			,IFNULL(A."NumAtCard",'') AS "RefInv"
		FROM TRIWALL_TRAINKEY."ODLN" AS A
		LEFT JOIN TRIWALL_TRAINKEY."OCPR" AS C ON A."CardCode"=C."CardCode" AND A."CntctCode"=C."CntctCode"
		LEFT JOIN TRIWALL_TRAINKEY."NNM1" AS F ON F."Series"=A."Series"
		WHERE 
			A."DocEntry"=:par1;
	ELSE IF :DTYPE='GET_Good_Return_Header_Detail_By_DocNum' THEN
		SELECT 
			 F."SeriesName" AS "SeriesName"
			,A."DocNum" AS "DocNum"
			,TO_VARCHAR(A."DocDate",'yyyy-mm-dd') AS "DocDate"
			,TO_VARCHAR(A."TaxDate",'yyyy-mm-dd') AS "TaxDate"
			,A."CardCode"|| ' - ' || A."CardName" AS "Vendor"
			,IFNULL(C."Name",'') AS "ContactPerson"
			,IFNULL(A."NumAtCard",'') AS "RefInv"
		FROM TRIWALL_TRAINKEY."ORPD" AS A
		LEFT JOIN TRIWALL_TRAINKEY."OCPR" AS C ON A."CardCode"=C."CardCode" AND A."CntctCode"=C."CntctCode"
		LEFT JOIN TRIWALL_TRAINKEY."NNM1" AS F ON F."Series"=A."Series"
		WHERE 
			A."DocEntry"=:par1;
	ELSE IF :DTYPE='GetReturnLineDetailByDocEntry' THEN
	
		SELECT 
			 B."LineNum" AS "BaseLineNumber"
			,B."DocEntry" AS "DocEntry"
			,B."ItemCode" AS "ItemCode"
			,B."Dscription" AS "ItemName"
			,B."OpenQty" AS "Qty"
			,B."Price" AS "Price"
			,B."LineTotal" AS "LineTotal"
			,B."TaxCode" AS "VatCode"
			,B."WhsCode" AS "WarehouseCode"
			,E."CodeBars" AS "BarCode"
			,CASE WHEN E."ManBtchNum"='Y' THEN
				'B'
			 WHEN E."ManSerNum"='Y' THEN
				'S'
			 ELSE
				'N'
			 END AS "ManageItem"
		FROM TRIWALL_TRAINKEY."ORDN" AS A
		LEFT JOIN TRIWALL_TRAINKEY."RDN1" AS B ON A."DocEntry"=B."DocEntry"
		LEFT JOIN TRIWALL_TRAINKEY."OCPR" AS C ON A."CardCode"=C."CardCode" AND A."CntctCode"=C."CntctCode"
		LEFT JOIN TRIWALL_TRAINKEY."OITM" AS E ON E."ItemCode"=B."ItemCode"
		WHERE A."DocEntry"=:par1;
		
	ELSE IF :DTYPE='GetGoodReturnLineDetailByDocEntry' THEN
	
		SELECT 
			 B."LineNum" AS "BaseLineNumber"
			,B."DocEntry" AS "DocEntry"
			,B."ItemCode" AS "ItemCode"
			,B."Dscription" AS "ItemName"
			,B."OpenQty" AS "Qty"
			,B."Price" AS "Price"
			,B."LineTotal" AS "LineTotal"
			,B."TaxCode" AS "VatCode"
			,B."WhsCode" AS "WarehouseCode"
			,E."CodeBars" AS "BarCode"
			,CASE WHEN E."ManBtchNum"='Y' THEN
				'B'
			 WHEN E."ManSerNum"='Y' THEN
				'S'
			 ELSE
				'N'
			 END AS "ManageItem"
		FROM TRIWALL_TRAINKEY."ORPD" AS A
		LEFT JOIN TRIWALL_TRAINKEY."RPD1" AS B ON A."DocEntry"=B."DocEntry"
		LEFT JOIN TRIWALL_TRAINKEY."OCPR" AS C ON A."CardCode"=C."CardCode" AND A."CntctCode"=C."CntctCode"
		LEFT JOIN TRIWALL_TRAINKEY."OITM" AS E ON E."ItemCode"=B."ItemCode"
		WHERE A."DocEntry"=:par1;
		
	ELSE IF :DTYPE='GetBatchSerialReturn' THEN
	
		SELECT ROW_NUMBER() OVER(ORDER BY "ItemCode") AS "LineNum",* FROM (
		
			SELECT 
				 A."ItemCode" AS "ItemCode"	
				,C."Quantity" AS "Qty"
				,B."DistNumber" AS "SerialBatch"
				,B."MnfSerial" AS "MfrSerialNo"
				,TO_VARCHAR(B."ExpDate",'yyyy-MM-dd') AS "ExpDate"
				,TO_VARCHAR(B."MnfDate",'yyyy-MM-dd') AS "MrfDate"
				,'Serial' AS "Type"
			FROM TRIWALL_TRAINKEY."SRI1" AS A
			LEFT JOIN TRIWALL_TRAINKEY."OSRN" AS B ON A."ItemCode"=B."ItemCode" AND B."SysNumber"=A."SysSerial"
			LEFT JOIN TRIWALL_TRAINKEY."OSRI" AS C On C."ItemCode"=A."ItemCode" And C."SysSerial"=A."SysSerial"
			WHERE A."BaseEntry"=:par1 
				AND A."BaseType"=16
				--And C."Status"<>0
					
			UNION ALL
			
			SELECT 
				 A."ItemCode" AS "ItemCode"	
				,A."Quantity" AS "Qty"
				,B."DistNumber" AS "SerialBatch"
				,B."MnfSerial" AS "MfrSerialNo"
				,TO_VARCHAR("ExpDate",'yyyy-MM-dd') AS "ExpDate"
				,TO_VARCHAR(B."MnfDate",'yyyy-MM-dd') AS "MrfDate"
				,'Batch' AS "Type"
			FROM TRIWALL_TRAINKEY."IBT1" AS A 
			LEFT JOIN TRIWALL_TRAINKEY."OBTN" AS B ON A."ItemCode"=B."ItemCode" AND A."BatchNum"=B."DistNumber"
			--LEFT JOIN TRIWALL_TRAINKEY."OBTQ" AS C ON C."ItemCode"=A."ItemCode" and B."SysNumber"=C."SysNumber"
			WHERE A."BaseEntry"=:par1
				AND A."BaseType"=16
		
		)AS A;
	ELSE IF :DTYPE='GetBatchSerialGoodReturn' THEN
	
		SELECT ROW_NUMBER() OVER(ORDER BY "ItemCode") AS "LineNum",* FROM (
		
			SELECT 
				 A."ItemCode" AS "ItemCode"	
				,C."Quantity" AS "Qty"
				,B."DistNumber" AS "SerialBatch"
				,B."MnfSerial" AS "MfrSerialNo"
				,TO_VARCHAR(B."ExpDate",'dd-MM-yyyy') AS "ExpDate"
				,TO_VARCHAR(B."MnfDate",'dd-MM-yyyy') AS "MrfDate"
				,'Serial' AS "Type"
			FROM TRIWALL_TRAINKEY."SRI1" AS A
			LEFT JOIN TRIWALL_TRAINKEY."OSRN" AS B ON A."ItemCode"=B."ItemCode" AND B."SysNumber"=A."SysSerial"
			LEFT JOIN TRIWALL_TRAINKEY."OSRI" AS C On C."ItemCode"=A."ItemCode" And C."SysSerial"=A."SysSerial"
			WHERE A."BaseEntry"=:par1 
				AND A."BaseType"=21
				--And C."Status"<>0
					
			UNION ALL
			
			SELECT 
				 A."ItemCode" AS "ItemCode"	
				,A."Quantity" AS "Qty"
				,B."DistNumber" AS "SerialBatch"
				,B."MnfSerial" AS "MfrSerialNo"
				,TO_VARCHAR("ExpDate",'dd-MM-yyyy') AS "ExpDate"
				,TO_VARCHAR(B."MnfDate",'dd-MM-yyyy') AS "MrfDate"
				,'Batch' AS "Type"
			FROM TRIWALL_TRAINKEY."IBT1" AS A 
			LEFT JOIN TRIWALL_TRAINKEY."OBTN" AS B ON A."ItemCode"=B."ItemCode" AND A."BatchNum"=B."DistNumber"
			--LEFT JOIN TRIWALL_TRAINKEY."OBTQ" AS C ON C."ItemCode"=A."ItemCode" and B."SysNumber"=C."SysNumber"
			WHERE A."BaseEntry"=:par1
				AND A."BaseType"=21
		
		)AS A;
	ELSE IF :DTYPE='GetDeliveryOrderLineForReturnDetailByDocEntry' THEN
	
		SELECT 
			 B."LineNum" AS "BaseLineNumber"
			,B."DocEntry" AS "DocEntry"
			,B."ItemCode" AS "ItemCode"
			,B."Dscription" AS "ItemName"
			,B."OpenQty" AS "Qty"
			,B."Price" AS "Price"
			,B."LineTotal" AS "LineTotal"
			,B."TaxCode" AS "VatCode"
			,B."WhsCode" AS "WarehouseCode"
			,E."CodeBars" AS "BarCode"
			,CASE WHEN E."ManBtchNum"='Y' THEN
				'B'
			 WHEN E."ManSerNum"='Y' THEN
				'S'
			 ELSE
				'N'
			 END AS "ManageItem"
		FROM TRIWALL_TRAINKEY."ODLN" AS A
		LEFT JOIN TRIWALL_TRAINKEY."DLN1" AS B ON A."DocEntry"=B."DocEntry"
		LEFT JOIN TRIWALL_TRAINKEY."OCPR" AS C ON A."CardCode"=C."CardCode" AND A."CntctCode"=C."CntctCode"
		LEFT JOIN TRIWALL_TRAINKEY."OITM" AS E ON E."ItemCode"=B."ItemCode"
		WHERE A."DocEntry"=:par1 AND B."LineStatus"='O';
	ELSE IF :DTYPE='GetGoodReceiptPOLineForGoodReturnDetailByDocEntry' THEN
	
		SELECT 
			 B."LineNum" AS "BaseLineNumber"
			,B."DocEntry" AS "DocEntry"
			,B."ItemCode" AS "ItemCode"
			,B."Dscription" AS "ItemName"
			,B."OpenQty" AS "Qty"
			,B."Price" AS "Price"
			,B."LineTotal" AS "LineTotal"
			,B."TaxCode" AS "VatCode"
			,B."WhsCode" AS "WarehouseCode"
			,E."CodeBars" AS "BarCode"
			,CASE WHEN E."ManBtchNum"='Y' THEN
				'B'
			 WHEN E."ManSerNum"='Y' THEN
				'S'
			 ELSE
				'N'
			 END AS "ManageItem"
		FROM TRIWALL_TRAINKEY."OPDN" AS A
		LEFT JOIN TRIWALL_TRAINKEY."PDN1" AS B ON A."DocEntry"=B."DocEntry"
		LEFT JOIN TRIWALL_TRAINKEY."OCPR" AS C ON A."CardCode"=C."CardCode" AND A."CntctCode"=C."CntctCode"
		LEFT JOIN TRIWALL_TRAINKEY."OITM" AS E ON E."ItemCode"=B."ItemCode"
		WHERE A."DocEntry"=:par1 AND B."LineStatus"='O';
		
	ELSE IF :DTYPE='ReturnDoHeader' THEN
	
		IF :par2='' THEN
			DECLARE offset INT;
			SELECT CAST(:par1 AS INT)*10 INTO offset FROM DUMMY;
	
			SELECT 
				 "DocEntry" AS "DocEntry"
				,"DocNum" AS "DocumentNumber"
				,"DocDate" AS "DocDate"
				,"CardCode" AS "VendorCode"
				,"Comments" AS "Remarks"
				,"TaxDate" AS "TaxDate"
			FROM TRIWALL_TRAINKEY."ORDN" 
			ORDER BY "DocEntry" LIMIT 10 OFFSET :offset;
		ELSE IF :par2='condition' THEN
		
			SELECT 
				 "DocEntry" AS "DocEntry"
				,"DocNum" AS "DocumentNumber"
				,"DocDate" AS "DocDate"
				,"CardCode" AS "VendorCode"
				,"Comments" AS "Remarks"
				,"TaxDate" AS "TaxDate"
			FROM TRIWALL_TRAINKEY."ORDN" 
			WHERE "DocStatus"='O'
			AND "DocDate" BETWEEN :par3 AND :par4
			AND "DocNum" LIKE CASE WHEN :par5='' OR :par5='0' THEN "DocNum" ELSE '%'||:par5||'%' END
			ORDER BY "DocEntry";
			
		END IF;
		END IF;
	ELSE IF :DTYPE='GoodReceiptPoHeaderReturnByDocEntry' THEN
		SELECT 
			 "DocEntry" AS "DocEntry"
			,"DocNum" AS "DocumentNumber"
			,TO_VARCHAR("DocDate",'yyyy-MM-dd') AS "DocDate"
			,"CardCode" AS "Vendor"
			,"Comments" AS "RefInv"
			,TO_VARCHAR("TaxDate",'yyyy-MM-dd') AS "TaxDate"
		FROM TRIWALL_TRAINKEY."OPDN" 
		WHERE "DocEntry"=:par1 AND "DocStatus"='O'
		ORDER BY "DocEntry";
	ELSE IF :DTYPE='GoodReceiptPOHeaderByReturn' THEN
	
		IF :par2='' THEN
			DECLARE offset INT;
			SELECT CAST(:par1 AS INT)*10 INTO offset FROM DUMMY;
	
			SELECT 
				 "DocEntry" AS "DocEntry"
				,"DocNum" AS "DocumentNumber"
				,TO_VARCHAR("DocDate",'yyyy-MM-dd') AS "DocDate"
				,"CardCode" AS "VendorCode"
				,"Comments" AS "Remarks"
				,TO_VARCHAR("TaxDate",'yyyy-MM-dd') AS "TaxDate"
			FROM TRIWALL_TRAINKEY."OPDN" 
			WHERE "DocStatus"='O'
			ORDER BY "DocEntry" LIMIT 10 OFFSET :offset;
		ELSE IF :par2='condition' THEN
		
			SELECT 
				 "DocEntry" AS "DocEntry"
				,"DocNum" AS "DocumentNumber"
				,TO_VARCHAR("DocDate",'yyyy-MM-dd') AS "DocDate"
				,"CardCode" AS "VendorCode"
				,"Comments" AS "Remarks"
				,TO_VARCHAR("TaxDate",'yyyy-MM-dd') AS "TaxDate"
			FROM TRIWALL_TRAINKEY."OPDN" 
			WHERE "DocStatus"='O' AND
				"DocDate" BETWEEN CASE WHEN :par3='' THEN '1999-01-01' ELSE :par3 END AND CASE WHEN :par4='' THEN '2100-01-01' ELSE :par4 END
			AND "DocNum" LIKE CASE WHEN :par5='' OR :par5='0' THEN "DocNum" ELSE '%'||:par5||'%' END
			ORDER BY "DocEntry";
			
		END IF;
		END IF;
	ELSE IF :DTYPE='GetBatchSerialDeliveryOrder' THEN
	
		SELECT ROW_NUMBER() OVER(ORDER BY "ItemCode") AS "LineNum",* FROM (
		
			SELECT 
				 A."ItemCode" AS "ItemCode"	
				,C."Quantity" AS "Qty"
				,B."DistNumber" AS "SerialBatch"
				,B."MnfSerial" AS "MfrSerialNo"
				,TO_VARCHAR(B."ExpDate",'yyyy-MM-dd') AS "ExpDate"
				,TO_VARCHAR(B."MnfDate",'yyyy-MM-dd') AS "MrfDate"
				,'Serial' AS "Type"
			FROM TRIWALL_TRAINKEY."SRI1" AS A
			LEFT JOIN TRIWALL_TRAINKEY."OSRN" AS B ON A."ItemCode"=B."ItemCode" AND B."SysNumber"=A."SysSerial"
			LEFT JOIN TRIWALL_TRAINKEY."OSRI" AS C On C."ItemCode"=A."ItemCode" And C."SysSerial"=A."SysSerial"
			WHERE A."BaseEntry"=:par1 
				AND A."BaseType"=15
				AND A."BaseLinNum"=CASE WHEN :par2='' THEN A."BaseLinNum" ELSE :par2 END
				--And C."Status"<>0
					
			UNION ALL
			
			SELECT 
				 A."ItemCode" AS "ItemCode"	
				,A."Quantity" AS "Qty"
				,B."DistNumber" AS "SerialBatch"
				,B."MnfSerial" AS "MfrSerialNo"
				,TO_VARCHAR("ExpDate",'yyyy-MM-dd') AS "ExpDate"
				,TO_VARCHAR(B."MnfDate",'yyyy-MM-dd') AS "MrfDate"
				,'Batch' AS "Type"
			FROM TRIWALL_TRAINKEY."IBT1" AS A 
			LEFT JOIN TRIWALL_TRAINKEY."OBTN" AS B ON A."ItemCode"=B."ItemCode" AND A."BatchNum"=B."DistNumber"
			--LEFT JOIN TRIWALL_TRAINKEY."OBTQ" AS C ON C."ItemCode"=A."ItemCode" and B."SysNumber"=C."SysNumber"
			WHERE A."BaseEntry"=:par1
				AND A."BaseType"=15
				AND A."BaseLinNum"=CASE WHEN :par2='' THEN A."BaseLinNum" ELSE :par2 END
		
		)AS A;
	ELSE IF :DTYPE='GetBatchSerialGoodReceiptPOForGoodReturn' THEN
	
		SELECT ROW_NUMBER() OVER(ORDER BY "ItemCode") AS "LineNum",* FROM (
		
			SELECT 
				 A."ItemCode" AS "ItemCode"	
				,C."Quantity" AS "Qty"
				,B."DistNumber" AS "SerialBatch"
				,B."MnfSerial" AS "MfrSerialNo"
				,TO_VARCHAR(B."ExpDate",'yyyy-MM-dd') AS "ExpDate"
				,TO_VARCHAR(B."MnfDate",'yyyy-MM-dd') AS "MrfDate"
				,'Serial' AS "Type"
			FROM TRIWALL_TRAINKEY."SRI1" AS A
			LEFT JOIN TRIWALL_TRAINKEY."OSRN" AS B ON A."ItemCode"=B."ItemCode" AND B."SysNumber"=A."SysSerial"
			LEFT JOIN TRIWALL_TRAINKEY."OSRI" AS C On C."ItemCode"=A."ItemCode" And C."SysSerial"=A."SysSerial"
			WHERE A."BaseEntry"=:par1 
				AND A."BaseType"=20
				AND A."BaseLinNum"=:par2
				--And C."Status"<>0
					
			UNION ALL
			
			SELECT 
				 A."ItemCode" AS "ItemCode"	
				,A."Quantity" AS "Qty"
				,B."DistNumber" AS "SerialBatch"
				,B."MnfSerial" AS "MfrSerialNo"
				,TO_VARCHAR("ExpDate",'yyyy-MM-dd') AS "ExpDate"
				,TO_VARCHAR(B."MnfDate",'yyyy-MM-dd') AS "MrfDate"
				,'Batch' AS "Type"
			FROM TRIWALL_TRAINKEY."IBT1" AS A 
			LEFT JOIN TRIWALL_TRAINKEY."OBTN" AS B ON A."ItemCode"=B."ItemCode" AND A."BatchNum"=B."DistNumber"
			--LEFT JOIN TRIWALL_TRAINKEY."OBTQ" AS C ON C."ItemCode"=A."ItemCode" and B."SysNumber"=C."SysNumber"
			WHERE A."BaseEntry"=:par1
				AND A."BaseType"=20
				AND A."BaseLinNum"=:par2
		
		)AS A;
	ELSE IF :DTYPE='GetARCreditMemoHeader' THEN
		IF :par2='' THEN
			DECLARE offset INT;
			SELECT CAST(:par1 AS INT)*10 INTO offset FROM DUMMY;
			SELECT 
				 "DocEntry" AS "DocEntry"
				,"DocNum" AS "DocumentNumber"
				,"DocDate" AS "DocDate"
				,"CardCode" AS "VendorCode"
				,"Comments" AS "Remarks"
				,"TaxDate" AS "TaxDate"
			FROM TRIWALL_TRAINKEY."ORIN" 
			ORDER BY "DocEntry" LIMIT 10 OFFSET :offset;
		ELSE IF :par2='condition' THEN
			SELECT 
				 "DocEntry" AS "DocEntry"
				,"DocNum" AS "DocumentNumber"
				,"DocDate" AS "DocDate"
				,"CardCode" AS "VendorCode"
				,"Comments" AS "Remarks"
				,"TaxDate" AS "TaxDate"
			FROM TRIWALL_TRAINKEY."ORIN" 
			WHERE "DocStatus"='O'
			AND "DocDate" BETWEEN :par3 AND :par4
			AND "DocNum" LIKE CASE WHEN :par5='' OR :par5='0' THEN "DocNum" ELSE '%'||:par5||'%' END
			ORDER BY "DocEntry";
		END IF;
		END IF;
	ELSE IF :DTYPE='GetARInvoiceInCreditMemo' THEN

		DECLARE offset INT;
		SELECT CAST(:par1 AS INT)*10 INTO offset FROM DUMMY;

		SELECT 
			 "DocEntry" AS "DocEntry"
			,"DocNum" AS "DocumentNumber"
			,"DocDate" AS "DocDate"
			,"CardCode" AS "VendorCode"
			,"Comments" AS "Remarks"
			,"TaxDate" AS "TaxDate"
		FROM TRIWALL_TRAINKEY."OINV" 
		WHERE "DocStatus"='O'
		ORDER BY "DocEntry" LIMIT 10 OFFSET :offset;
	ELSE IF :DTYPE='GET_AR_Credit_Memo_Header_Detail_By_DocNum' THEN
	
		SELECT 
			 F."SeriesName" AS "SeriesName"
			,A."DocNum" AS "DocNum"
			,TO_VARCHAR(A."DocDate",'yyyy-mm-dd') AS "DocDate"
			,TO_VARCHAR(A."TaxDate",'yyyy-mm-dd') AS "TaxDate"
			,A."CardCode"|| ' - ' || A."CardName" AS "Vendor"
			,IFNULL(C."Name",'') AS "ContactPerson"
			,IFNULL(A."NumAtCard",'') AS "RefInv"
		FROM TRIWALL_TRAINKEY."ORIN" AS A
		LEFT JOIN TRIWALL_TRAINKEY."OCPR" AS C ON A."CardCode"=C."CardCode" AND A."CntctCode"=C."CntctCode"
		LEFT JOIN TRIWALL_TRAINKEY."NNM1" AS F ON F."Series"=A."Series"
		WHERE 
			A."DocEntry"=:par1;
	ELSE IF :DTYPE='GetARCreditMemoLineDetailByDocEntry' THEN
	
		SELECT 
			 B."LineNum" AS "BaseLineNumber"
			,B."DocEntry" AS "DocEntry"
			,B."ItemCode" AS "ItemCode"
			,B."Dscription" AS "ItemName"
			,B."OpenQty" AS "Qty"
			,B."Price" AS "Price"
			,B."LineTotal" AS "LineTotal"
			,B."TaxCode" AS "VatCode"
			,B."WhsCode" AS "WarehouseCode"
			,E."CodeBars" AS "BarCode"
			,CASE WHEN E."ManBtchNum"='Y' THEN
				'B'
			 WHEN E."ManSerNum"='Y' THEN
				'S'
			 ELSE
				'N'
			 END AS "ManageItem"
		FROM TRIWALL_TRAINKEY."ORIN" AS A
		LEFT JOIN TRIWALL_TRAINKEY."RIN1" AS B ON A."DocEntry"=B."DocEntry"
		LEFT JOIN TRIWALL_TRAINKEY."OCPR" AS C ON A."CardCode"=C."CardCode" AND A."CntctCode"=C."CntctCode"
		LEFT JOIN TRIWALL_TRAINKEY."OITM" AS E ON E."ItemCode"=B."ItemCode"
		WHERE A."DocEntry"=:par1;
		
	ELSE IF :DTYPE='GetBatchSerialARCreditMemo' THEN
	
		SELECT ROW_NUMBER() OVER(ORDER BY "ItemCode") AS "LineNum",* FROM (
		
			SELECT 
				 A."ItemCode" AS "ItemCode"	
				,C."Quantity" AS "Qty"
				,B."DistNumber" AS "SerialBatch"
				,B."MnfSerial" AS "MfrSerialNo"
				,TO_VARCHAR(B."ExpDate",'yyyy-MM-dd') AS "ExpDate"
				,TO_VARCHAR(B."MnfDate",'yyyy-MM-dd') AS "MrfDate"
				,'Serial' AS "Type"
			FROM TRIWALL_TRAINKEY."SRI1" AS A
			LEFT JOIN TRIWALL_TRAINKEY."OSRN" AS B ON A."ItemCode"=B."ItemCode" AND B."SysNumber"=A."SysSerial"
			LEFT JOIN TRIWALL_TRAINKEY."OSRI" AS C On C."ItemCode"=A."ItemCode" And C."SysSerial"=A."SysSerial"
			WHERE A."BaseEntry"=:par1 
				AND A."BaseType"=14
				--And C."Status"<>0
					
			UNION ALL
			
			SELECT 
				 A."ItemCode" AS "ItemCode"	
				,A."Quantity" AS "Qty"
				,B."DistNumber" AS "SerialBatch"
				,B."MnfSerial" AS "MfrSerialNo"
				,TO_VARCHAR("ExpDate",'yyyy-MM-dd') AS "ExpDate"
				,TO_VARCHAR(B."MnfDate",'yyyy-MM-dd') AS "MrfDate"
				,'Batch' AS "Type"
			FROM TRIWALL_TRAINKEY."IBT1" AS A 
			LEFT JOIN TRIWALL_TRAINKEY."OBTN" AS B ON A."ItemCode"=B."ItemCode" AND A."BatchNum"=B."DistNumber"
			--LEFT JOIN TRIWALL_TRAINKEY."OBTQ" AS C ON C."ItemCode"=A."ItemCode" and B."SysNumber"=C."SysNumber"
			WHERE A."BaseEntry"=:par1
				AND A."BaseType"=14
		
		)AS A;
	ELSE IF :DTYPE='GetARInvoiceLineForARCreditMemoDetailByDocEntry' THEN
	
		SELECT 
			 B."LineNum" AS "BaseLineNumber"
			,B."DocEntry" AS "DocEntry"
			,B."ItemCode" AS "ItemCode"
			,B."Dscription" AS "ItemName"
			,B."OpenQty" AS "Qty"
			,B."Price" AS "Price"
			,B."LineTotal" AS "LineTotal"
			,B."TaxCode" AS "VatCode"
			,B."WhsCode" AS "WarehouseCode"
			,E."CodeBars" AS "BarCode"
			,CASE WHEN E."ManBtchNum"='Y' THEN
				'B'
			 WHEN E."ManSerNum"='Y' THEN
				'S'
			 ELSE
				'N'
			 END AS "ManageItem"
		FROM TRIWALL_TRAINKEY."OINV" AS A
		LEFT JOIN TRIWALL_TRAINKEY."INV1" AS B ON A."DocEntry"=B."DocEntry"
		LEFT JOIN TRIWALL_TRAINKEY."OCPR" AS C ON A."CardCode"=C."CardCode" AND A."CntctCode"=C."CntctCode"
		LEFT JOIN TRIWALL_TRAINKEY."OITM" AS E ON E."ItemCode"=B."ItemCode"
		WHERE A."DocEntry"=:par1 AND B."LineStatus"='O';
	ELSE IF :DTYPE='GetBatchSerialARInvoiceForARCreditMemo' THEN
	
		SELECT ROW_NUMBER() OVER(ORDER BY "ItemCode") AS "LineNum",* FROM (
		
			SELECT 
				 A."ItemCode" AS "ItemCode"	
				,1 AS "Qty"
				,B."DistNumber" AS "SerialBatch"
				,B."MnfSerial" AS "MfrSerialNo"
				,TO_VARCHAR(B."ExpDate",'yyyy-MM-dd') AS "ExpDate"
				,TO_VARCHAR(B."MnfDate",'yyyy-MM-dd') AS "MrfDate"
				,'Serial' AS "Type"
			FROM TRIWALL_TRAINKEY."SRI1" AS A
			LEFT JOIN TRIWALL_TRAINKEY."OSRN" AS B ON A."ItemCode"=B."ItemCode" AND B."SysNumber"=A."SysSerial"
			LEFT JOIN TRIWALL_TRAINKEY."OSRI" AS C On C."ItemCode"=A."ItemCode" And C."SysSerial"=A."SysSerial"
			WHERE A."BaseEntry"=:par1 
				AND A."BaseType"=13
				AND A."BaseLinNum"=:par2
				AND B."DistNumber" NOT IN (
					SELECT 
						T1."DistNumber"
					FROM TRIWALL_TRAINKEY."SRI1" AS T0
					LEFT JOIN TRIWALL_TRAINKEY."OSRN" AS T1 ON T0."ItemCode"=T1."ItemCode" AND T1."SysNumber"=T0."SysSerial"
					LEFT JOIN TRIWALL_TRAINKEY."ORIN" AS T2 ON T0."BaseEntry"=T2."DocEntry"
					 WHERE 	T0."BsDocType"='13' 
					 		AND T0."BaseType"='14'
					 		AND T2."CANCELED"='N'
				)
				--And C."Status"<>0
					
			UNION ALL
			
			SELECT 
				 A."ItemCode" AS "ItemCode"	
				,A."Quantity" AS "Qty"
				,B."DistNumber" AS "SerialBatch"
				,B."MnfSerial" AS "MfrSerialNo"
				,TO_VARCHAR("ExpDate",'yyyy-MM-dd') AS "ExpDate"
				,TO_VARCHAR(B."MnfDate",'yyyy-MM-dd') AS "MrfDate"
				,'Batch' AS "Type"
			FROM TRIWALL_TRAINKEY."IBT1" AS A 
			LEFT JOIN TRIWALL_TRAINKEY."OBTN" AS B ON A."ItemCode"=B."ItemCode" AND A."BatchNum"=B."DistNumber"
			--LEFT JOIN TRIWALL_TRAINKEY."OBTQ" AS C ON C."ItemCode"=A."ItemCode" and B."SysNumber"=C."SysNumber"
			WHERE A."BaseEntry"=:par1
				AND A."BaseType"=13
				AND A."BaseLinNum"=:par2
				/*AND B."DistNumber" NOT IN (
					SELECT 
						T1."DistNumber"
					FROM TRIWALL_TRAINKEY."IBT1" AS T0
					LEFT JOIN TRIWALL_TRAINKEY."OBTN" AS T1 ON T0."ItemCode"=T1."ItemCode" AND T1."DistNumber"=T0."BatchNum"
					LEFT JOIN TRIWALL_TRAINKEY."ORIN" AS T2 ON T0."BaseEntry"=T2."DocEntry"
					 WHERE 	T0."BsDocType"='13' 
					 		AND T0."BaseType"='14'
					 		AND T2."CANCELED"='N'
				)*/
		
		)AS A;
	ELSE IF :DTYPE='ARCreditMemoHeader' THEN
	
		IF :par2='' THEN
			DECLARE offset INT;
			SELECT CAST(:par1 AS INT)*10 INTO offset FROM DUMMY;
	
			SELECT 
				 "DocEntry" AS "DocEntry"
				,"DocNum" AS "DocumentNumber"
				,"DocDate" AS "DocDate"
				,"CardCode" AS "VendorCode"
				,"Comments" AS "Remarks"
				,"TaxDate" AS "TaxDate"
			FROM TRIWALL_TRAINKEY."ORIN" 
			ORDER BY "DocEntry" LIMIT 10 OFFSET :offset;
		ELSE IF :par2='condition' THEN
		
			SELECT 
				 "DocEntry" AS "DocEntry"
				,"DocNum" AS "DocumentNumber"
				,"DocDate" AS "DocDate"
				,"CardCode" AS "VendorCode"
				,"Comments" AS "Remarks"
				,"TaxDate" AS "TaxDate"
			FROM TRIWALL_TRAINKEY."ORIN" 
			WHERE "DocStatus"='O'
			AND "DocDate" BETWEEN :par3 AND :par4
			AND "DocNum" LIKE CASE WHEN :par5='' OR :par5='0' THEN "DocNum" ELSE '%'||:par5||'%' END
			ORDER BY "DocEntry";
			
		END IF;
		END IF;
	ELSE IF :DTYPE='GetInventoryCountingList' THEN
		SELECT 
			 "DocEntry" AS "DocEntry"
			,"DocNum" AS "Series"
			,LEFT(CASE
				WHEN LENGTH("Time") = 3  THEN CAST('0' || "Time" AS TIME) 
				WHEN LENGTH("Time") = 2 THEN CAST('00' || "Time" AS TIME)
				WHEN LENGTH("Time") = 1 THEN CAST('000' || "Time" AS TIME) 
				ELSE CAST("Time" AS TIME) END ,5) AS "CreateTime"
			,TO_VARCHAR("CountDate",'yyyy-MM-dd') AS "CreateDate"
			,"Remarks" AS "OtherRemark"
			,"Ref2" AS "Ref2"
			,CASE WHEN "CountType"=1 THEN
				'Single Count'
			 ELSE
			 	'Multiple Count'
			 END AS "InventoryCountingType"
		FROM TRIWALL_TRAINKEY."OINC"
		WHERE "Status"='O';
	ELSE IF :DTYPE='GetInventoryCountingLine' THEN
		SELECT 
			 A."ItemCode" AS "ItemCode"
			,A."ItemDesc" AS "ItemName"
			,A."InWhsQty" AS "Qty"
			,A."Counted" AS "Counted"
			,A."LineNum" AS "LineNum"
			,CASE WHEN B."ManBtchNum"='Y' THEN
				'B'
			 WHEN B."ManSerNum"='Y' THEN
				'S'
			 ELSE
				'N'
			 END AS "ItemType"
			,'' AS "CountId"
			,A."BinEntry" AS "BinEntry"
			,A."UomCode" AS "Uom"
			,A."WhsCode" AS "WarehouseCode"
		FROM TRIWALL_TRAINKEY."INC1" AS A
		LEFT JOIN TRIWALL_TRAINKEY."OITM" AS B ON A."ItemCode"=B."ItemCode"
		WHERE A."DocEntry"=:par1;
		
	ELSE IF :DTYPE='GET_InventoryTransfer_Header_Detail_By_DocNum' THEN
	
		SELECT 
			 F."SeriesName" AS "SeriesName"
			,A."DocNum" AS "DocNum"
			,TO_VARCHAR(A."DocDate",'yyyy-mm-dd') AS "DocDate"
			,TO_VARCHAR(A."TaxDate",'yyyy-mm-dd') AS "TaxDate"
			,A."CardCode"|| ' - ' || A."CardName" AS "Vendor"
			,IFNULL(C."Name",'') AS "ContactPerson"
			,IFNULL(A."NumAtCard",'') AS "RefInv"
			,A."Filler" AS "WhsFrom"
			,A."ToWhsCode" AS "WhsTo"
		FROM TRIWALL_TRAINKEY."OWTR" AS A
		LEFT JOIN TRIWALL_TRAINKEY."OCPR" AS C ON A."CardCode"=C."CardCode" AND A."CntctCode"=C."CntctCode"
		LEFT JOIN TRIWALL_TRAINKEY."NNM1" AS F ON F."Series"=A."Series"
		WHERE 
			A."DocEntry"=:par1;
			
	ELSE IF :DTYPE='GetInventoryTransferLineDetailByDocEntry' THEN
		SELECT 
			 B."LineNum" AS "BaseLineNumber"
			,B."DocEntry" AS "DocEntry"
			,B."ItemCode" AS "ItemCode"
			,B."Dscription" AS "ItemName"
			,B."OpenQty" AS "Qty"
			,B."Price" AS "Price"
			,B."LineTotal" AS "LineTotal"
			,B."TaxCode" AS "VatCode"
			,B."WhsCode" AS "WarehouseCode"
			,E."CodeBars" AS "BarCode"
			,CASE WHEN E."ManBtchNum"='Y' THEN
				'B'
			 WHEN E."ManSerNum"='Y' THEN
				'S'
			 ELSE
				'N'
			 END AS "ManageItem"
		FROM TRIWALL_TRAINKEY."OWTR" AS A
		LEFT JOIN TRIWALL_TRAINKEY."WTR1" AS B ON A."DocEntry"=B."DocEntry"
		LEFT JOIN TRIWALL_TRAINKEY."OCPR" AS C ON A."CardCode"=C."CardCode" AND A."CntctCode"=C."CntctCode"
		LEFT JOIN TRIWALL_TRAINKEY."OITM" AS E ON E."ItemCode"=B."ItemCode"
		WHERE A."DocEntry"=:par1;
	ELSE IF :DTYPE='GetBatchSerialInventoryTransfer' THEN
	
		SELECT ROW_NUMBER() OVER(ORDER BY "ItemCode") AS "LineNum",* FROM (
		
			SELECT DISTINCT
				 A."ItemCode" AS "ItemCode"	
				,C."Quantity" AS "Qty"
				,B."DistNumber" AS "SerialBatch"
				,B."MnfSerial" AS "MfrSerialNo"
				,TO_VARCHAR(B."ExpDate",'yyyy-MM-dd') AS "ExpDate"
				,TO_VARCHAR(B."MnfDate",'yyyy-MM-dd') AS "MrfDate"
				,'Serial' AS "Type"
			FROM TRIWALL_TRAINKEY."SRI1" AS A
			LEFT JOIN TRIWALL_TRAINKEY."OSRN" AS B ON A."ItemCode"=B."ItemCode" AND B."SysNumber"=A."SysSerial"
			LEFT JOIN TRIWALL_TRAINKEY."OSRI" AS C On C."ItemCode"=A."ItemCode" And C."SysSerial"=A."SysSerial"
			WHERE A."BaseEntry"=:par1 
				AND A."BaseType"=67
				--AND A."BaseLinNum"=:par2
				--And C."Status"<>0
					
			UNION ALL
			
			SELECT DISTINCT
				 A."ItemCode" AS "ItemCode"	
				,A."Quantity" AS "Qty"
				,B."DistNumber" AS "SerialBatch"
				,B."MnfSerial" AS "MfrSerialNo"
				,TO_VARCHAR("ExpDate",'yyyy-MM-dd') AS "ExpDate"
				,TO_VARCHAR(B."MnfDate",'yyyy-MM-dd') AS "MrfDate"
				,'Batch' AS "Type"
			FROM TRIWALL_TRAINKEY."IBT1" AS A 
			LEFT JOIN TRIWALL_TRAINKEY."OBTN" AS B ON A."ItemCode"=B."ItemCode" AND A."BatchNum"=B."DistNumber"
			--LEFT JOIN TRIWALL_TRAINKEY."OBTQ" AS C ON C."ItemCode"=A."ItemCode" and B."SysNumber"=C."SysNumber"
			WHERE A."BaseEntry"=:par1
				AND A."BaseType"=67
				--AND A."BaseLinNum"=:par2
		
		)AS A;
	ELSE IF :DTYPE='GetInventoryTransferHeader' THEN

		IF :par2='' THEN
			DECLARE offset INT;
			SELECT CAST(:par1 AS INT)*10 INTO offset FROM DUMMY;
			SELECT 
				 "DocEntry" AS "DocEntry"
				,"DocNum" AS "DocumentNumber"
				,TO_VARCHAR("DocDate",'yyyy-MM-dd') AS "DocDate"
				,"CardCode" AS "VendorCode"
				,"Comments" AS "Remarks"
				,TO_VARCHAR("TaxDate",'yyyy-MM-dd') AS "TaxDate"
			FROM TRIWALL_TRAINKEY."OWTR" 
			ORDER BY "DocEntry" LIMIT 10 OFFSET :offset;
		ELSE IF :par2='condition' THEN
			SELECT 
				 "DocEntry" AS "DocEntry"
				,"DocNum" AS "DocumentNumber"
				,TO_VARCHAR("DocDate",'yyyy-MM-dd') AS "DocDate"
				,"CardCode" AS "VendorCode"
				,"Comments" AS "Remarks"
				,TO_VARCHAR("TaxDate",'yyyy-MM-dd') AS "TaxDate"
			FROM TRIWALL_TRAINKEY."OWTR" 
			WHERE "DocStatus"='O'
			AND "DocDate" BETWEEN CASE WHEN :par3='' THEN '1999-01-01' ELSE :par3 END AND CASE WHEN :par4='' THEN '2100-01-01' ELSE :par4 END
			AND "DocNum" LIKE CASE WHEN :par5='' OR :par5='0' THEN "DocNum" ELSE '%'||:par5||'%' END
			ORDER BY "DocEntry";
		END IF;
		END IF;
	ELSE IF :DTYPE='GET_InventoryCounting_Header_Detail_By_DocNum' THEN
		SELECT 
			 "DocNum" AS "DocNum"
			,TO_VARCHAR("CountDate",'yyyy-MM-dd') AS "CreateDate"
			,LEFT(CASE
				WHEN LENGTH("Time") = 3  THEN CAST('0' || "Time" AS TIME) 
				WHEN LENGTH("Time") = 2 THEN CAST('00' || "Time" AS TIME)
				WHEN LENGTH("Time") = 1 THEN CAST('000' || "Time" AS TIME) 
				ELSE CAST("Time" AS TIME) END ,5) AS "CreateTime"	
			,"Ref2" AS "Ref2"
		FROM TRIWALL_TRAINKEY."OINC"
		WHERE "DocEntry"=:par1;
	ELSE IF :DTYPE='GetInventoryCountingLineDetailByDocEntry' THEN
		SELECT 
			 B."LineNum" AS "LineNum"
			,B."ItemCode" AS "ItemCode"
			,E."ItemName" AS "ItemName"
			,B."WhsCode" AS "WarehouseCode"
			,B."InWhsQty" AS "Qty"
			,B."CountQty" AS "QtyCounted"
			,CASE WHEN E."ManBtchNum"='Y' THEN
				'B'
			 WHEN E."ManSerNum"='Y' THEN
				'S'
			 ELSE
				'N'
			 END AS "ManageItem"
		FROM TRIWALL_TRAINKEY."OINC" AS A
		LEFT JOIN TRIWALL_TRAINKEY."INC1" AS B ON A."DocEntry"=B."DocEntry"
		LEFT JOIN TRIWALL_TRAINKEY."OITM" AS E ON E."ItemCode"=B."ItemCode"
		WHERE A."DocEntry"=:par1;
	ELSE IF :DTYPE='GetBatchSerialInventoryCounting' THEN
		SELECT ROW_NUMBER() OVER(ORDER BY "ItemCode") AS "LineNum",* FROM (
			SELECT 
				 A."ItemCode" AS "ItemCode"	
				,1 AS "Qty"
				,B."DistNumber" AS "SerialBatch"
				,B."MnfSerial" AS "MfrSerialNo"
				,TO_VARCHAR(B."ExpDate",'dd-MM-yyyy') AS "ExpDate"
				,B."MnfDate" AS "MrfDate"
				,'Serial' AS "Type"
			FROM TRIWALL_TRAINKEY."SRI1" AS A
			LEFT JOIN TRIWALL_TRAINKEY."OSRN" AS B ON A."ItemCode"=B."ItemCode" AND B."SysNumber"=A."SysSerial"
			LEFT JOIN TRIWALL_TRAINKEY."OSRI" AS C On C."ItemCode"=A."ItemCode" And C."SysSerial"=A."SysSerial"
			WHERE A."BaseEntry"=:par1 
				AND A."BaseType"=1470000065 
				--And C."Status"<>0
					
			UNION ALL
			
			SELECT 
				 A."ItemCode" AS "ItemCode"	
				,A."Quantity" AS "Qty"
				,B."DistNumber" AS "SerialBatch"
				,B."MnfSerial" AS "MfrSerialNo"
				,TO_VARCHAR("ExpDate",'dd-MM-yyyy') AS "ExpDate"
				,B."MnfDate" AS "MrfDate"
				,'Batch' AS "Type"
			FROM TRIWALL_TRAINKEY."IBT1" AS A 
			LEFT JOIN TRIWALL_TRAINKEY."OBTN" AS B ON A."ItemCode"=B."ItemCode" AND A."BatchNum"=B."DistNumber"
			--LEFT JOIN TRIWALL_TRAINKEY."OBTQ" AS C ON C."ItemCode"=A."ItemCode" and B."SysNumber"=C."SysNumber"
			WHERE A."BaseEntry"=:par1
				AND A."BaseType"=1470000065
		)AS A;
	ELSE IF :DTYPE='InventoryCounting' THEN
	
		IF :par2='' THEN
			DECLARE offset INT;
			SELECT CAST(:par1 AS INT)*10 INTO offset FROM DUMMY;

			SELECT DISTINCT
				 A."DocEntry" AS "DocEntry"
				,A."DocNum" AS "DocumentNumber"
				,TO_VARCHAR(A."CountDate",'yyyy-MM-dd') AS "DocDate"
				,CASE WHEN A."CountType"=1 THEN
					'Single Count'
				 ELSE
				 	'Multiple Count'
				 END AS "VendorCode"
				,A."Ref2" AS "Remarks"
				,TO_VARCHAR(A."CountDate",'yyyy-MM-dd') AS "TaxDate"
			FROM TRIWALL_TRAINKEY."OINC" AS A
			LEFT JOIN TRIWALL_TRAINKEY."INC1" AS B ON B."DocEntry"=A."DocEntry" 
			--WHERE B."BaseType"='1470000065'
			ORDER BY A."DocEntry" LIMIT 10 OFFSET :offset;
		ELSE IF :par2='condition' THEN
			SELECT DISTINCT
				 A."DocEntry" AS "DocEntry"
				,A."DocNum" AS "DocumentNumber"
				,TO_VARCHAR(A."CountDate",'yyyy-MM-dd') AS "DocDate"
				,CASE WHEN A."CountType"=1 THEN
					'Single Count'
				 ELSE
				 	'Multiple Count'
				 END AS "VendorCode"
				,A."Ref2" AS "Remarks"
				,TO_VARCHAR(A."CountDate",'yyyy-MM-dd') AS "TaxDate"
			FROM TRIWALL_TRAINKEY."OINC" AS A
			LEFT JOIN TRIWALL_TRAINKEY."INC1" AS B ON B."DocEntry"=A."DocEntry" 
			WHERE
			--AND B."BaseType"='1470000065'
			--AND 
			--A."CountDate" BETWEEN :par3 AND :par4
			A."CountDate" BETWEEN CASE WHEN :par3='' THEN '1999-01-01' ELSE :par3 END AND CASE WHEN :par4='' THEN '2100-01-01' ELSE :par4 END
			AND A."DocNum" LIKE CASE WHEN :par5='' OR :par5='0' THEN "DocNum" ELSE '%'||:par5||'%' END
			ORDER BY "DocEntry";
		END IF;
		END IF;
	
	ELSE IF :DTYPE='CallLayout' THEN
		SELECT 
			  A."U_FILENAME" AS FILENAME
			 ,A."U_EXPORTTYPE" AS EXPORTTYPE
			 ,A."U_STOREPROCEDURE" AS STOREPROCEDURE
			 ,A."U_PROPERTIES" AS PROPERTIES
			 ,IFNULL(A."U_LayoutPrintName",'') AS LAYOUTPRINTNAME
		FROM TRIWALL_TRAINKEY."@TBREPORT" AS A WHERE A."Code"=:par1;
	ELSE IF :DTYPE='GET_Production_Finished_Good' THEN
		execute immediate '
			SELECT  
				 A."DocEntry" AS "DocEntry"
				--,A."LineNum" AS "OrderLineNum"
				,0 AS "OrderLineNum"
				,A."ItemCode" AS "ItemCode"
				,B."ItemName" AS "ItemName"
				,A."PlannedQty"-A."CmpltQty" AS "Qty"
				,A."PlannedQty" AS "PlanQty"
				,C."UgpCode" AS "Uom"
				,A."Warehouse" AS "WarehouseCode"
				,CASE WHEN B."ManSerNum"=''Y'' THEN
				 	''S''
				 WHEN B."ManBtchNum"=''Y'' THEN
				 	''B''
				 ELSE ''N'' END AS "ItemType"
			FROM TRIWALL_TRAINKEY."OWOR" AS A
			LEFT JOIN TRIWALL_TRAINKEY."OITM" AS B ON B."ItemCode"=A."ItemCode"
			LEFT JOIN TRIWALL_TRAINKEY."OUGP" AS C ON B."UgpEntry"=C."UgpEntry"
			WHERE A."DocEntry" IN ('|| :par1 ||') AND (A."PlannedQty"-A."CmpltQty")<>0;';
	ELSE IF :DTYPE='GetReturnRequestHeader' THEN
		IF :par2='' THEN
			DECLARE offset INT;
			SELECT CAST(:par1 AS INT)*10 INTO offset FROM DUMMY;
			SELECT 
				 "DocEntry" AS "DocEntry"
				,"DocNum" AS "DocumentNumber"
				,TO_VARCHAR("DocDate",'yyyy-MM-dd') AS "DocDate"
				,"CardCode" AS "VendorCode"
				,"Comments" AS "Remarks"
				,TO_VARCHAR("TaxDate",'yyyy-MM-dd') AS "TaxDate"
			FROM TRIWALL_TRAINKEY."ORRR" 
			ORDER BY "DocEntry" LIMIT 10 OFFSET :offset;
		ELSE IF :par2='condition' THEN
			SELECT 
				 "DocEntry" AS "DocEntry"
				,"DocNum" AS "DocumentNumber"
				,TO_VARCHAR("DocDate",'yyyy-MM-dd') AS "DocDate"
				,"CardCode" AS "VendorCode"
				,"Comments" AS "Remarks"
				,TO_VARCHAR("TaxDate",'yyyy-MM-dd') AS "TaxDate"
			FROM TRIWALL_TRAINKEY."ORRR" 
			WHERE "DocStatus"='O'
			AND "DocDate" BETWEEN CASE WHEN :par3='' THEN '1999-01-01' ELSE :par3 END AND CASE WHEN :par4='' THEN '2100-01-01' ELSE :par4 END
			AND "DocNum" LIKE CASE WHEN :par5='' OR :par5='0' THEN "DocNum" ELSE '%'||:par5||'%' END
			ORDER BY "DocEntry";
		END IF;
		END IF;
	ELSE IF :DTYPE='GetDeliveryOrderReturnRequest' THEN

		DECLARE offset INT;
		SELECT CAST(:par1 AS INT)*10 INTO offset FROM DUMMY;

		SELECT 
			 "DocEntry" AS "DocEntry"
			,"DocNum" AS "DocumentNumber"
			,TO_VARCHAR("DocDate",'yyyy-MM-dd') AS "DocDate"
			,"CardCode" AS "VendorCode"
			,"Comments" AS "Remarks"
			,TO_VARCHAR("TaxDate",'yyyy-MM-dd') AS "TaxDate"
		FROM TRIWALL_TRAINKEY."ODLN" 
		WHERE "DocStatus"='O'
		ORDER BY "DocEntry" LIMIT 10 OFFSET :offset;
	ELSE IF :DTYPE='GET_Return_Request_Header_Detail_By_DocNum' THEN
		SELECT 
			 F."SeriesName" AS "SeriesName"
			,A."DocNum" AS "DocNum"
			,TO_VARCHAR(A."DocDate",'yyyy-mm-dd') AS "DocDate"
			,TO_VARCHAR(A."TaxDate",'yyyy-mm-dd') AS "TaxDate"
			,A."CardCode"|| ' - ' || A."CardName" AS "Vendor"
			,IFNULL(C."Name",'') AS "ContactPerson"
			,IFNULL(A."NumAtCard",'') AS "RefInv"
		FROM TRIWALL_TRAINKEY."ORRR" AS A
		LEFT JOIN TRIWALL_TRAINKEY."OCPR" AS C ON A."CardCode"=C."CardCode" AND A."CntctCode"=C."CntctCode"
		LEFT JOIN TRIWALL_TRAINKEY."NNM1" AS F ON F."Series"=A."Series"
		WHERE 
			A."DocEntry"=:par1;
	ELSE IF :DTYPE='GetReturnRequestLineDetailByDocEntry' THEN
	
		SELECT 
			 B."LineNum" AS "BaseLineNumber"
			,B."DocEntry" AS "DocEntry"
			,B."ItemCode" AS "ItemCode"
			,B."Dscription" AS "ItemName"
			,B."OpenQty" AS "Qty"
			,B."Price" AS "Price"
			,B."LineTotal" AS "LineTotal"
			,B."TaxCode" AS "VatCode"
			,B."WhsCode" AS "WarehouseCode"
			,E."CodeBars" AS "BarCode"
			,CASE WHEN E."ManBtchNum"='Y' THEN
				'B'
			 WHEN E."ManSerNum"='Y' THEN
				'S'
			 ELSE
				'N'
			 END AS "ManageItem"
		FROM TRIWALL_TRAINKEY."ORRR" AS A
		LEFT JOIN TRIWALL_TRAINKEY."RRR1" AS B ON A."DocEntry"=B."DocEntry"
		LEFT JOIN TRIWALL_TRAINKEY."OCPR" AS C ON A."CardCode"=C."CardCode" AND A."CntctCode"=C."CntctCode"
		LEFT JOIN TRIWALL_TRAINKEY."OITM" AS E ON E."ItemCode"=B."ItemCode"
		WHERE A."DocEntry"=:par1;
		
	ELSE IF :DTYPE='GetBatchSerialReturnRequest' THEN
	
		SELECT ROW_NUMBER() OVER(ORDER BY "ItemCode") AS "LineNum",* FROM (
		
			SELECT 
				 A."ItemCode" AS "ItemCode"	
				,C."Quantity" AS "Qty"
				,B."DistNumber" AS "SerialBatch"
				,B."MnfSerial" AS "MfrSerialNo"
				,TO_VARCHAR(B."ExpDate",'yyyy-MM-dd') AS "ExpDate"
				,TO_VARCHAR(B."MnfDate",'yyyy-MM-dd') AS "MrfDate"
				,'Serial' AS "Type"
			FROM TRIWALL_TRAINKEY."SRI1" AS A
			LEFT JOIN TRIWALL_TRAINKEY."OSRN" AS B ON A."ItemCode"=B."ItemCode" AND B."SysNumber"=A."SysSerial"
			LEFT JOIN TRIWALL_TRAINKEY."OSRI" AS C On C."ItemCode"=A."ItemCode" And C."SysSerial"=A."SysSerial"
			WHERE A."BaseEntry"=:par1 
				AND A."BaseType"=234000031
				--And C."Status"<>0
					
			UNION ALL
			
			SELECT 
				 A."ItemCode" AS "ItemCode"	
				,A."Quantity" AS "Qty"
				,B."DistNumber" AS "SerialBatch"
				,B."MnfSerial" AS "MfrSerialNo"
				,TO_VARCHAR("ExpDate",'yyyy-MM-dd') AS "ExpDate"
				,TO_VARCHAR(B."MnfDate",'yyyy-MM-dd') AS "MrfDate"
				,'Batch' AS "Type"
			FROM TRIWALL_TRAINKEY."IBT1" AS A 
			LEFT JOIN TRIWALL_TRAINKEY."OBTN" AS B ON A."ItemCode"=B."ItemCode" AND A."BatchNum"=B."DistNumber"
			--LEFT JOIN TRIWALL_TRAINKEY."OBTQ" AS C ON C."ItemCode"=A."ItemCode" and B."SysNumber"=C."SysNumber"
			WHERE A."BaseEntry"=:par1
				AND A."BaseType"=234000031
		
		)AS A;
	ELSE IF :DTYPE='GET_Delivery_Order_Header_Detail_By_DocNum_Return_Request' THEN
		SELECT 
			 F."SeriesName" AS "SeriesName"
			,A."DocNum" AS "DocNum"
			,TO_VARCHAR(A."DocDate",'yyyy-mm-dd') AS "DocDate"
			,TO_VARCHAR(A."TaxDate",'yyyy-mm-dd') AS "TaxDate"
			,A."CardCode" AS "Vendor"
			,IFNULL(C."Name",'') AS "ContactPerson"
			,IFNULL(A."NumAtCard",'') AS "RefInv"
		FROM TRIWALL_TRAINKEY."ODLN" AS A
		LEFT JOIN TRIWALL_TRAINKEY."OCPR" AS C ON A."CardCode"=C."CardCode" AND A."CntctCode"=C."CntctCode"
		LEFT JOIN TRIWALL_TRAINKEY."NNM1" AS F ON F."Series"=A."Series"
		WHERE 
			A."DocEntry"=:par1;
	ELSE IF :DTYPE='GetDeliveryOrderLineForReturnRequestDetailByDocEntry' THEN
	
		SELECT 
			 B."LineNum" AS "BaseLineNumber"
			,B."DocEntry" AS "DocEntry"
			,B."ItemCode" AS "ItemCode"
			,B."Dscription" AS "ItemName"
			,B."OpenQty" AS "Qty"
			,B."Price" AS "Price"
			,B."LineTotal" AS "LineTotal"
			,B."VatGroup" AS "VatCode"
			,B."WhsCode" AS "WarehouseCode"
			,E."CodeBars" AS "BarCode"
			,CASE WHEN E."ManBtchNum"='Y' THEN
				'B'
			 WHEN E."ManSerNum"='Y' THEN
				'S'
			 ELSE
				'N'
			 END AS "ManageItem"
		FROM TRIWALL_TRAINKEY."ODLN" AS A
		LEFT JOIN TRIWALL_TRAINKEY."DLN1" AS B ON A."DocEntry"=B."DocEntry"
		LEFT JOIN TRIWALL_TRAINKEY."OCPR" AS C ON A."CardCode"=C."CardCode" AND A."CntctCode"=C."CntctCode"
		LEFT JOIN TRIWALL_TRAINKEY."OITM" AS E ON E."ItemCode"=B."ItemCode"
		WHERE A."DocEntry"=:par1 AND B."LineStatus"='O';
	
	ELSE IF :DTYPE='ReturnRequestDoHeader' THEN
	
		IF :par2='' THEN
			DECLARE offset INT;
			SELECT CAST(:par1 AS INT)*10 INTO offset FROM DUMMY;
	
			SELECT 
				 "DocEntry" AS "DocEntry"
				,"DocNum" AS "DocumentNumber"
				,"DocDate" AS "DocDate"
				,"CardCode" AS "VendorCode"
				,"Comments" AS "Remarks"
				,"TaxDate" AS "TaxDate"
			FROM TRIWALL_TRAINKEY."ODLN" 
			ORDER BY "DocEntry" LIMIT 10 OFFSET :offset;
			
		ELSE IF :par2='condition' THEN
		
			SELECT 
				 "DocEntry" AS "DocEntry"
				,"DocNum" AS "DocumentNumber"
				,TO_VARCHAR("DocDate",'yyyy-MM-dd') AS "DocDate"
				,"CardCode" AS "VendorCode"
				,"Comments" AS "Remarks"
				,TO_VARCHAR("TaxDate",'yyyy-MM-dd') AS "TaxDate"
			FROM TRIWALL_TRAINKEY."ODLN" 
			WHERE "DocStatus"='O'
			--AND "DocDate" BETWEEN :par3 AND :par4
			AND "DocDate" BETWEEN CASE WHEN :par3='' THEN '1999-01-01' ELSE :par3 END AND CASE WHEN :par4='' THEN '2100-01-01' ELSE :par4 END
			AND "DocNum" LIKE CASE WHEN :par5='' OR :par5='0' THEN "DocNum" ELSE '%'||:par5||'%' END
			ORDER BY "DocEntry";
			
		END IF;
		END IF;
	ELSE IF :DTYPE='JwtCheckAccount' THEN
		IF :par1='admin' AND :par2='1234' THEN
			SELECT 
				 :par1 AS "Account"
				,:par2 AS "Password"
			From DUMMY;
		END IF;
	ELSE IF :DTYPE='LayoutPrinter' THEN
		SELECT "Code","Name" FROM TRIWALL_TRAINKEY."@TBREPORT" WHERE "U_LAYOUTMODULE"=:par1;
	END IF;
	END IF;
	END IF;
	END IF;
	END IF;
	END IF;
	END IF;
	END IF;
	END IF;
	END IF;
	END IF;
	END IF;
	END IF;
	END IF;
	END IF;
	END IF;
	END IF;
	END IF;
	END IF;
	END IF;
	END IF;
	END IF;
	END IF;
	END IF;
	END IF;
	END IF;
	END IF;	
	END IF;
	END IF;
	END IF;
	END IF;	
	END IF;	
	END IF;
	END IF;
	END IF;	
	END IF;	
	END IF;
	END IF;
	END IF;
	END IF;
	END IF;
	END IF;
	END IF;
	END IF;
	END IF;		
	END IF;		
	END IF;
	END IF;
	END IF;
	END IF;
	END IF;
	END IF;
	END IF;
	END IF;
	END IF;
	END IF;
	END IF;
	END IF;
	END IF;
	END IF;
	END IF;
	END IF;
	END IF;
	END IF;
	END IF;
	END IF;
	END IF;
	END IF;
	END IF;
	END IF;
	END IF;
	END IF;
	END IF;
	END IF;
	END IF;
	END IF;
	END IF;
	END IF;
	END IF;
	END IF;
	END IF;
	END IF;
	END IF;
	END IF;
	END IF;
	END IF;
	END IF;
	END IF;
	END IF;
	END IF;
	END IF;
	END IF;
	END IF;
	END IF;
	END IF;
	END IF;
	END IF;
	END IF;
	END IF;
	END IF;
	END IF;
	END IF;
	END IF;
	END IF;
	END IF;
	END IF;
	END IF;
	END IF;
	END IF;
	END IF;
	END IF;
	END IF;
	END IF;
	END IF;
	END IF;
	END IF;
	END IF;
	END IF;
	END IF;
	END IF;
	END IF;
	END IF;
	END IF;
	END IF;
	END IF;
	END IF;
	END IF;
	END IF;
	END IF;
	END IF;
	END IF;
	END IF;
	END IF;
	END IF;
	END IF;
	END IF;
	END IF;
	END IF;
	END IF;
	END IF;
	END IF;
	END IF;
	END IF;
	END IF;
	END IF;
	END IF;
	END IF;
	END IF;
	END IF;
	END IF;
	END IF;
	END IF;
	END IF;
	END IF;
	END IF;
	END IF;
	END IF;
	END IF;
	END IF;
	END IF;
	END IF;
	END IF;
	END IF;
	END IF;
	END IF;
	END IF;
	END IF;
	END IF;
	END IF;
	END IF;
	END IF;
	END IF;
	END IF;
	END IF;
	END IF;
	END IF;
	END IF;
	END IF;
	END IF;
	END IF;
	END IF;
END;
