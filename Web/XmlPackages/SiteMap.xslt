<?xml version="1.0" encoding="utf-8"?>
<xsl:stylesheet version="1.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform">
  <xsl:template match='/'>
    <ul>
      <xsl:for-each select='SiteMap/node'>
        <xsl:call-template name='nodeTemplate'>
          <xsl:with-param name='node' select='.' />
        </xsl:call-template>
      </xsl:for-each>
    </ul>
  </xsl:template>
  <xsl:template name='nodeTemplate'>
    <xsl:param name='node' />
    <li>
      <a>
        <xsl:attribute name='href'>
          <xsl:value-of select='$node/@NavigateUrl' />
        </xsl:attribute>
        <xsl:value-of select='$node/@Text' />
      </a>
      <xsl:for-each select='$node/node'>
        <ul>
          <xsl:call-template name='nodeTemplate'>
            <xsl:with-param name='node' select='.' />
          </xsl:call-template>
        </ul>
      </xsl:for-each>
    </li>
  </xsl:template>
</xsl:stylesheet>