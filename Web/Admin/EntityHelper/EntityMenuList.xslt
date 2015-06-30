<?xml version="1.0" encoding="UTF-8" ?>

	<xsl:stylesheet version="1.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform">
	<xsl:output method="xml" standalone="no" omit-xml-declaration="yes"/>

	<xsl:param name="entity"></xsl:param>
	<xsl:param name="ForParentEntityID"></xsl:param>
    <xsl:param name="custlocale"></xsl:param>
	<xsl:param name="deflocale"></xsl:param>
	<xsl:param name="adminsite"></xsl:param>

	<xsl:template match="root">
		<xsl:param name="prefix">
			<xsl:choose>
				<xsl:when test="$entity='Category'"><xsl:value-of select="'c'" /></xsl:when>
				<xsl:when test="$entity='Section'"><xsl:value-of select="'s'" /></xsl:when>
				<xsl:when test="$entity='Manufacturer'"><xsl:value-of select="'m'" /></xsl:when>
				<xsl:when test="$entity='Library'"><xsl:value-of select="'l'" /></xsl:when>
			</xsl:choose>
		</xsl:param>

        <xsl:choose>
            <xsl:when test="$ForParentEntityID=0">
                <xsl:apply-templates select="Entity">
                    <xsl:with-param name="prefix" select="$prefix"/>
                </xsl:apply-templates>
            </xsl:when>
            <xsl:otherwise>
                <xsl:apply-templates select="descendant-or-self::Entity[EntityID=$ForParentEntityID]" >
                    <xsl:with-param name="prefix" select="$prefix"/>
                </xsl:apply-templates>
            </xsl:otherwise>
        </xsl:choose>

	</xsl:template>

	<xsl:template match="Entity">
		<xsl:param name="prefix"/>
		<xsl:param name="eName">
			<xsl:choose>
				<xsl:when test="count(Name/ml/locale[@name=$custlocale])!=0">
					<xsl:value-of select="Name/ml/locale[@name=$custlocale]"/>
				</xsl:when>
				<xsl:when test="count(Name/ml/locale[@name=$deflocale])!=0">
					<xsl:value-of select="Name/ml/locale[@name=$deflocale]"/>
				</xsl:when>
				<xsl:when test="count(Name/ml)=0">
					<xsl:value-of select="Name"/>
				</xsl:when>
			</xsl:choose>
		</xsl:param>

		<item>
			<xsl:attribute name="Text"><xsl:value-of select="$eName"/></xsl:attribute>
			<xsl:attribute name="NavigateUrl"><xsl:value-of select="concat($prefix, '-', EntityID, '-', SEName)"/>.aspx</xsl:attribute>
			<xsl:choose>
				<xsl:when test="$adminsite=0">
                    <xsl:attribute name="NavigateUrl"><xsl:value-of select="concat($prefix, '-', EntityID, '-', SEName)"/>.aspx</xsl:attribute>
                </xsl:when>
				<xsl:otherwise><xsl:attribute name="NavigateUrl">newentities.aspx?<xsl:value-of select="$entity"/>id=<xsl:value-of select="EntityID"/></xsl:attribute></xsl:otherwise>
			</xsl:choose>
			<xsl:if test="boolean(child::Entity)">
				<xsl:attribute name="Look-RightIconUrl">arrow.gif</xsl:attribute>
				<xsl:attribute name="Look-RightIconWidth">15</xsl:attribute>
			</xsl:if>
			<xsl:apply-templates select="Entity">
				<xsl:with-param name="prefix" select="$prefix"/>
			</xsl:apply-templates>
		</item>

	</xsl:template>

</xsl:stylesheet>
