<?xml version="1.0" encoding="UTF-8" ?>

<xsl:stylesheet version="1.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform">
	<xsl:output method="html" standalone="yes"/>

	<xsl:param name="entity"></xsl:param>
	<xsl:param name="ForParentEntityID"></xsl:param>
	<xsl:param name="StoreLoc"></xsl:param>

	<xsl:template match="root">
        <xsl:choose>
        	<xsl:when test="$ForParentEntityID=0"><xsl:apply-templates select="Entity" /></xsl:when>
        	<xsl:otherwise><xsl:apply-templates select="descendant-or-self::Entity[EntityID=$ForParentEntityID]" /></xsl:otherwise>
        </xsl:choose>
		
	</xsl:template>

	<xsl:template match="Entity">
		<sitemap>
			<loc>
				<xsl:value-of select="$StoreLoc"/>googleentity.aspx?entityname=<xsl:value-of select="$entity"/>&amp;entityid=<xsl:value-of select="EntityID"/>
			</loc>
		</sitemap>
		<xsl:apply-templates select="Entity" />
	</xsl:template>

</xsl:stylesheet>