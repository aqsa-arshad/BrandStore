<?xml version="1.0" encoding="utf-8"?>
<xsl:stylesheet version="1.0"
				xmlns:xsl="http://www.w3.org/1999/XSL/Transform"
				xmlns:msxsl="urn:schemas-microsoft-com:xslt"
				xmlns:aspdnsf="urn:aspdnsf"
				xmlns:mobile="urn:mobile"
				exclude-result-prefixes="msxsl aspdnsf mobile ">
	<xsl:output method="html" indent="yes"/>
	
	<xsl:param name="ImageWidth" select="aspdnsf:AppConfig('Mobile.Entity.ImageWidth')" />
	<xsl:param name="SkinID" select="/root/Runtime/SkinID" />
	<xsl:param name="DisplayOutOfStockOnEntityPages" select="aspdnsf:AppConfigBool('DisplayOutOfStockOnEntityPages')" />
	<xsl:param name="OutOfStockThreshold" select="aspdnsf:AppConfig('OutOfStockThreshold')" />

	<!-- Product -->
  <xsl:template match="Product">
    <xsl:param name="pName">
      <xsl:choose>
        <xsl:when test="count(Name/ml/locale[@name=$LocaleSetting])!=0">
          <xsl:value-of select="Name/ml/locale[@name=$LocaleSetting]"/>
        </xsl:when>
        <xsl:when test="count(Name/ml/locale[@name=$WebConfigLocaleSetting])!=0">
          <xsl:value-of select="Name/ml/locale[@name=$WebConfigLocaleSetting]"/>
        </xsl:when>
        <xsl:when test="count(Name/ml)=0">
          <xsl:value-of select="Name"/>
        </xsl:when>
      </xsl:choose>
    </xsl:param>
    <xsl:param name="vName">
      <xsl:choose>
        <xsl:when test="count(VariantName/ml/locale[@name=$LocaleSetting])!=0">
          <xsl:value-of select="Name/ml/locale[@name=$LocaleSetting]"/>
        </xsl:when>
        <xsl:when test="count(VariantName/ml/locale[@name=$WebConfigLocaleSetting])!=0">
          <xsl:value-of select="VariantName/ml/locale[@name=$WebConfigLocaleSetting]"/>
        </xsl:when>
        <xsl:when test="count(VariantName/ml)=0">
          <xsl:value-of select="VariantName"/>
        </xsl:when>
      </xsl:choose>
    </xsl:param>
    <xsl:param name="pSalesPromptName">
      <xsl:choose>
        <xsl:when test="count(/root/Products/Product/SalesPromptName/ml/locale[@name=$LocaleSetting])!=0">
          <xsl:value-of select="/root/Products/Product/SalesPromptName/ml/locale[@name=$LocaleSetting]"/>
        </xsl:when>
        <xsl:when test="count(/root/Products/Product/SalesPromptName/ml/locale[@name=$WebConfigLocaleSetting])!=0">
          <xsl:value-of select="/root/Products/Product/SalesPromptName/ml/locale[@name=$WebConfigLocaleSetting]"/>
        </xsl:when>
        <xsl:when test="count(/root/Products/Product/SalesPromptName/ml)=0">
          <xsl:value-of select="/root/Products/Product/SalesPromptName"/>
        </xsl:when>
      </xsl:choose>
    </xsl:param>
    <xsl:param name="imageSource">
      <xsl:value-of select="aspdnsf:LookupProductImage(ProductID, ImageFilenameOverride, SKU, 'icon', 0, '')" disable-output-escaping="yes" />
    </xsl:param>


    <li>
      <a href="{aspdnsf:ProductLink(ProductID, SEName, 0, '')}" class="productListFullLink">
        <xsl:text disable-output-escaping="yes" ><![CDATA[<img src="]]></xsl:text>
        <xsl:value-of select="aspdnsf:ProductImageUrl(ProductID, ImageFileNameOverride, SKU, 'icon', 1)" />
        <xsl:text disable-output-escaping="yes" ><![CDATA[" alt="]]></xsl:text>
        <xsl:value-of select="$pName" />
        <xsl:text disable-output-escaping="yes" ><![CDATA[" class="productListItemImage" width="]]></xsl:text>
        <xsl:value-of select="$ImageWidth" />
        <xsl:text disable-output-escaping="yes" ><![CDATA[" />]]></xsl:text>
        <h3>
          <xsl:value-of select="$pName" disable-output-escaping="yes" />
          <xsl:if test="vName!=''">
            -<xsl:value-of select="$vName" />
          </xsl:if>
        </h3>
        <p>
          <xsl:value-of select="aspdnsf:GetVariantPrice(VariantID, HidePriceUntilCart, Price, SalePrice, ExtendedPrice, 0, SalesPromptName)" disable-output-escaping="yes" />
        </p>
		<xsl:if test="$DisplayOutOfStockOnEntityPages = 'true'">
			<xsl:choose>
				<xsl:when test="aspdnsf:AppConfigBool('ShowInventoryTable')='true'">
					<div class="productListItemStockHint">
						<xsl:value-of select="aspdnsf:ShowInventoryTable(ProductID, VariantID)" disable-output-escaping="yes" />
					</div>
				</xsl:when>
				<xsl:otherwise>
					<xsl:if test="$DisplayOutOfStockOnEntityPages = 'true'">
						<div class="productListItemStockHint">
							<xsl:value-of select="aspdnsf:DisplayProductStockHint(ProductID, VariantID, 'Product')" disable-output-escaping="yes" />
						</div>
					</xsl:if>
				</xsl:otherwise>
			</xsl:choose>
		</xsl:if>
      </a>
      <a href="{aspdnsf:ProductLink(ProductID, SEName, 0, '')}">
        View Product
      </a>
    </li>
  </xsl:template>

</xsl:stylesheet>
