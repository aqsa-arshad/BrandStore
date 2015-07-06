<?xml version="1.0"?>

<xsl:stylesheet version="1.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform">
	<xsl:output method="text" omit-xml-declaration="yes" indent="no" standalone="yes"/>
    <xsl:param name="locale"></xsl:param>

    <xsl:template match="root">
		<xsl:comment><xsl:value-of select="'RecordType'"/>,</xsl:comment>
		<xsl:value-of select="'ProductID (do not edit)'"/>,<xsl:value-of select="'VariantID (do not edit)'"/>,<xsl:value-of select="'KitItemID'"/>,<xsl:value-of select="'ProductName'"/>,<xsl:value-of select="'KitGroup'"/>,<xsl:value-of select="'SKU'"/>,<xsl:value-of select="'SKUSuffix'"/>,<xsl:value-of select="'MaufacturerPartNumber'"/>,<xsl:value-of select="'Cost'"/>,<xsl:value-of select="'MSRP'"/>,<xsl:value-of select="'Price'"/>,<xsl:value-of select="'SalePrice'"/>,<xsl:value-of select="'Inventory'"/><xsl:text>&#13;&#10;</xsl:text>
		<xsl:apply-templates select="product" />
	</xsl:template>

	<xsl:template match="product">
        <xsl:param name="pName">
            <xsl:choose>
                <xsl:when test="Name/ml">
                    <xsl:value-of select="Name/ml/locale[@name=$locale]"/>
                </xsl:when>
                <xsl:otherwise>
                    <xsl:value-of select="Name"/>
                </xsl:otherwise>
            </xsl:choose>
        </xsl:param>
        <xsl:variable name="pKitGroup">
            <xsl:choose>
                <xsl:when test="KitGroup/ml">
                    <xsl:value-of select="KitGroup/ml/locale[@name=$locale]" />
                </xsl:when>
                <xsl:otherwise>
                    <xsl:value-of select="KitGroup" />
                </xsl:otherwise>
            </xsl:choose>
        </xsl:variable>
        <xsl:value-of select="ProductID"/>,<xsl:value-of select="VariantID"/>,<xsl:value-of select="KitItemID"/>,&quot;<xsl:value-of select="translate($pName, ',', '')"/>&quot;,&quot;<xsl:value-of select="translate($pKitGroup, ',', '')"/>&quot;,&quot;<xsl:value-of select="SKU"/>&quot;,&quot;<xsl:value-of select="SKUSuffix"/>&quot;,&quot;<xsl:value-of select="ManufacturerPartNumber"/>&quot;,<xsl:value-of select="Cost"/>,<xsl:value-of select="MSRP"/>,<xsl:value-of select="Price"/>,<xsl:value-of select="SalePrice"/>,<xsl:value-of select="Inventory"/><xsl:text>&#13;&#10;</xsl:text>
    </xsl:template>

</xsl:stylesheet>