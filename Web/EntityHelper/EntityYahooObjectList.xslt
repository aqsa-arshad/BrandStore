<?xml version="1.0"?>

<xsl:stylesheet version="1.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform">
	<xsl:output method="html" omit-xml-declaration="yes"/>
	<xsl:param name="entityPrefix"></xsl:param>
	<xsl:param name="objectPrefix"></xsl:param>
    <xsl:param name="storeBaseUrl"></xsl:param>

    <xsl:template match="root">
        <xsl:for-each select="object">
            <xsl:value-of select="$storeBaseUrl"/><xsl:value-of select="$objectPrefix"/><xsl:value-of select="$entityPrefix"/>-<xsl:value-of select="ObjectID"/>-<xsl:value-of select="EntityID"/>-<xsl:value-of select="SEName"/><xsl:value-of select="'.aspx&#0013;&#0010;'"/>
		</xsl:for-each>
	</xsl:template>

</xsl:stylesheet>