<?xml version="1.0"?>

<xsl:stylesheet version="1.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform">
    <xsl:param name="locale"></xsl:param>
    <xsl:template match="/">
        <root>
            <xsl:for-each select="/root/product">
                <xsl:variable name="pname">
                    <xsl:choose>
                    	<xsl:when test="Name/ml"><xsl:value-of select="Name/ml/locale[@name=$locale]" /></xsl:when>
                    	<xsl:otherwise><xsl:value-of select="Name" /></xsl:otherwise>
                    </xsl:choose>
                </xsl:variable>
                <xsl:variable name="pKitGroup">
                    <xsl:choose>
                    	<xsl:when test="KitGroup/ml"><xsl:value-of select="KitGroup/ml/locale[@name=$locale]" /></xsl:when>
                    	<xsl:otherwise><xsl:value-of select="KitGroup" /></xsl:otherwise>
                    </xsl:choose>
                </xsl:variable>
                <product>
                    <xsl:for-each select="*">
                        <xsl:choose>
                        	<xsl:when test="name() = 'Name'"><Name><xsl:value-of select="$pname" /></Name></xsl:when>
                        	<xsl:when test="name() = 'KitGroup'"><KitGroup><xsl:value-of select="$pKitGroup" /></KitGroup></xsl:when>
                        	<xsl:when test="name() != 'KitGroupID'"><xsl:copy-of select="."/></xsl:when>
                        </xsl:choose>
                    </xsl:for-each>
                </product>
            </xsl:for-each>
        </root>
    </xsl:template>
</xsl:stylesheet>