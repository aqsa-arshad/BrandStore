<?xml version="1.0"?>

<xsl:stylesheet version="1.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform">
	<xsl:output method="xml" version="1.0" omit-xml-declaration="yes" indent="yes" standalone="yes"/>
    <xsl:param name="locale"></xsl:param>

    <xsl:template match="root">
		<productlist>
			<xsl:apply-templates select="product" />
		</productlist>
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
        <productvariant>
            <xsl:copy-of select="ProductID"/>
            <xsl:copy-of select="VariantID"/>
            <xsl:copy-of select="KitItemID"/>
            <Name>
                <xsl:value-of select="$pName"/>
            </Name>
            <KitGroup>
                <xsl:value-of select="$pKitGroup" />
            </KitGroup>
            <xsl:copy-of select="SKU"/>
            <xsl:copy-of select="SKUSuffix"/>
            <xsl:copy-of select="ManufacturerPartNumber"/>
            <xsl:copy-of select="Cost"/>
            <xsl:copy-of select="MSRP"/>
            <xsl:copy-of select="Price"/>
            <xsl:copy-of select="SalePrice"/>
            <xsl:copy-of select="Inventory"/>
        </productvariant>
	</xsl:template>

	<xsl:template match="kitgroup">
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
        <kitgroup>
            <xsl:attribute name="KitGroupID"><xsl:value-of select="KitGroupID" /></xsl:attribute>
            <xsl:attribute name="Name"><xsl:value-of select="$pName" /></xsl:attribute>
            <xsl:apply-templates select="kititem" />
		</kitgroup>
	</xsl:template>

	<xsl:template match="kititem">
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
        <kititem>
			<xsl:copy-of select="KitItemID"/>
            <Name><xsl:value-of select="$pName"/></Name>
            <xsl:copy-of select="PriceDelta"/>
		</kititem>
	</xsl:template>

	
</xsl:stylesheet>