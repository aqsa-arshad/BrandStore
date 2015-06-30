<?xml version="1.0" encoding="UTF-8" ?>

<xsl:stylesheet version="1.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform">
	<xsl:output method="xml" omit-xml-declaration="yes" indent="no" />
	
	<xsl:param name="entity"></xsl:param>
	<xsl:param name="ForParentEntityID"></xsl:param>
	<xsl:param name="entityDispName"></xsl:param>
	<xsl:param name="adminsite"></xsl:param>
    <xsl:param name="custlocale"></xsl:param>
	<xsl:param name="deflocale"></xsl:param>
	<xsl:param name="expandID"></xsl:param>

	<xsl:template match="root">
		<xsl:param name="prefix">
			<xsl:choose>
				<xsl:when test="$entity='Category'"><xsl:value-of select="'c'" /></xsl:when>
				<xsl:when test="$entity='Section'"><xsl:value-of select="'s'" /></xsl:when>
				<xsl:when test="$entity='Manufacturer'"><xsl:value-of select="'m'" /></xsl:when>
				<xsl:when test="$entity='Library'"><xsl:value-of select="'l'" /></xsl:when>
			</xsl:choose>
		</xsl:param>
        
		<Nodes>
            <xsl:attribute name="Text"><xsl:value-of select="$entityDispName"/></xsl:attribute>
            <xsl:attribute name="Expanded">true</xsl:attribute>
            <xsl:attribute name="NavigateUrl">entityBulkDisplayOrder.aspx?EntityName=<xsl:value-of select="$entity"/>&amp;entityid=<xsl:value-of select="EntityID"/></xsl:attribute>
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
            <TreeViewNode>
                <xsl:attribute name="Text">&lt;font color='red'&gt;Add <xsl:value-of select="$entity"/>&lt;/font&gt;</xsl:attribute>
                <xsl:attribute name="NavigateUrl">entityEdit.aspx?entityparent=0&amp;entityname=<xsl:value-of select="$entity"/>&amp;entityid=0</xsl:attribute>
            </TreeViewNode>
        </Nodes>
    
	</xsl:template>

	<xsl:template match="Entity">
		<xsl:param name="prefix"></xsl:param>
		<xsl:param name="eName">
			<xsl:choose>
				<xsl:when test="count(Name/ml/locale[@name=$custlocale])!=0 and Name/ml/locale[@name=$custlocale] != ''">
					<xsl:value-of select="Name/ml/locale[@name=$custlocale]"/>
				</xsl:when>
				<xsl:when test="count(Name/ml/locale[@name=$deflocale])!=0 and Name/ml/locale[@name=$deflocale] != ''">
					<xsl:value-of select="Name/ml/locale[@name=$deflocale]"/>
				</xsl:when>
				<xsl:when test="count(Name/ml)=0">
					<xsl:value-of select="Name"/>
				</xsl:when>
                <xsl:otherwise>[Not Set for this Locale]</xsl:otherwise>
			</xsl:choose>
		</xsl:param>
    
        <TreeViewNode>
            <xsl:if test="boolean(descendant-or-self::Entity[./EntityID=$expandID])">
                <xsl:attribute name="Expanded">true</xsl:attribute>
            </xsl:if>

            <xsl:attribute name="NavigateUrl">entityEdit.aspx?entityname=<xsl:value-of select="$entity"/>&amp;entityid=<xsl:value-of select="EntityID"/></xsl:attribute>
            <xsl:attribute name="Text"><xsl:value-of select="$eName"/></xsl:attribute>
            <xsl:attribute name="ContentCallbackUrl">x-EntitySubmenu.aspx?entityname=<xsl:value-of select="$entity"/>&amp;parentid=<xsl:value-of select="EntityID"/></xsl:attribute>

            <xsl:if test="boolean(descendant-or-self::Entity[./EntityID=$expandID])">
            <xsl:apply-templates select="Entity">
                <xsl:with-param name="prefix" select="$prefix"/>
            </xsl:apply-templates>


            <!-- ADD NEW -->
            <TreeViewNode>
                <xsl:attribute name="Text">&lt;font color='red'&gt;Add <xsl:value-of select="$entity"/>&lt;/font&gt;</xsl:attribute>
                <xsl:attribute name="NavigateUrl">entityEdit.aspx?entityparent=<xsl:value-of select="EntityID"/>&amp;entityname=<xsl:value-of select="$entity"/>&amp;entityid=0</xsl:attribute>
            </TreeViewNode>

            <!-- PRODUCTS -->
            <TreeViewNode>
                <xsl:attribute name="Text">
                    &lt;a target=entityBody href=entityProducts.aspx?entityname=<xsl:value-of select="$entity"/>&amp;EntityFilterID=<xsl:value-of select="EntityID"/>&gt;
                    &lt;font color='green'&gt;Products&lt;/font&gt;
                    &lt;/a&gt;
                </xsl:attribute>
                <xsl:attribute name="ContentCallbackUrl">XmlEntityProducts.aspx?entityname=<xsl:value-of select="$entity"/>&amp;entityid=<xsl:value-of select="EntityID"/></xsl:attribute>
            </TreeViewNode>
            </xsl:if>
                  
          </TreeViewNode>
  </xsl:template>

</xsl:stylesheet>
