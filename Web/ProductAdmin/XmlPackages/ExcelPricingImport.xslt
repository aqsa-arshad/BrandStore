<?xml version="1.0"?>

<xsl:stylesheet version="1.0" 
    xmlns:xsl="http://www.w3.org/1999/XSL/Transform" 
    xmlns:aspdnsf="urn:aspdnsf" 
    exclude-result-prefixes="aspdnsf">

	<xsl:template match="/">
		<productlist>
			<xsl:apply-templates select="/excel/sheet/row[@id>1]"></xsl:apply-templates>
		</productlist>
	</xsl:template>
	
	<xsl:template match="row">
		<productvariant>
			<ProductID><xsl:value-of select="col[@id='A']"/></ProductID>
			<VariantID><xsl:value-of select="col[@id='B']"/></VariantID>
            <KitItemID><xsl:value-of select="col[@id='C']"/></KitItemID>
            <Name><xsl:value-of select="col[@id='D']"/></Name>
            <KitGroup><xsl:value-of select="col[@id='E']" /></KitGroup>
            <SKU><xsl:value-of select="col[@id='F']"/></SKU>
            <SKUSuffix><xsl:value-of select="col[@id='H']"/></SKUSuffix>
            <ManufacturerPartNumber><xsl:value-of select="col[@id='G']"/></ManufacturerPartNumber>
            <Cost><xsl:value-of select="aspdnsf:ParseDBDecimal(col[@id='I'])"/></Cost>
			<MSRP><xsl:value-of select="aspdnsf:ParseDBDecimal(col[@id='J'])"/></MSRP>
			<Price><xsl:value-of select="aspdnsf:ParseDBDecimal(col[@id='K'])"/></Price>
			<SalePrice><xsl:value-of select="aspdnsf:ParseDBDecimal(col[@id='L'])"/></SalePrice>
			<Inventory><xsl:value-of select="col[@id='M']"/></Inventory>
        </productvariant>
	</xsl:template>
</xsl:stylesheet>