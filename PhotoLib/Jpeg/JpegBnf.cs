namespace PhotoLib.Jpeg
{
    class JpegBnf
    {
        /// byte groupings
        // <anyb>       → BY00 | … | BYFF
        // <non00b>     → BY01 | … | BYFF
        // <nonFFb>     → BY00 | … | BYFE
        // <indexb>     → BY00 | … | BY3F
        // <kxindexb>   → BY01 | … | BY3F
        // <spechi0b>   → BY00 | BY01 | BY02 | BY03
        // <spechi1b>   → BY10 | BY11 | BY12 | BY13
        // <nsofb>      → BYC0 | BYC1 | BYC2 | BYC3 | BYC9 | BYCA | BYCB
        // <dsofb>      → BYC5 | BYC6 | BYC7 | BYCD | BYCE | BYCF
        // <rstmb>      → BYD0 | … | BYD7
        // <appnb>      → BYE0 | … | BYEF
        // <jpgnb>      → BYF0 | … | BYFD
        // <resb>       → BY02 | … | BYBF

        /// markers
        // <fill>       → ϵ 
        //              | <fill> BYFF
        // <prefix>     → <fill> BYFF
        // <appn>       → <prefix> <appnb>
        // <com>        → <prefix> BYFE
        // <dac>        → <prefix> BYCC
        // <dhp>        → <prefix> BYDE
        // <dht>        → <prefix> BYC4
        // <dnl>        → <prefix> BYDC
        // <dqt>        → <prefix> BYDB
        // <dri>        → <prefix> BYDD
        // <dsof>       → <prefix> <dsofb>
        // <eoi>        → <prefix> BYD9
        // <exp>        → <prefix> BYDF
        // <jpg>        → <prefix> BYC8
        // <jpgn>       → <prefix> <jpgnb>
        // <nsof>       → <prefix> <nsofb>
        // <rstm>       → <prefix> <rstmb>
        // <soi>        → <prefix> BYD8
        // <sos>        → <prefix> BYDA
        // <tem>        → <prefix> BY01
        // <forbid>     → <prefix> <resb> 
        //              | <tem> 
        //              | <jpg> 
        //              | <jpgn>

        /// tables and miscellaneous marker segments
        // <intval>     → <anyb> <anyb>
        // <len>        → <intval>
        // <qbyt_spec>  → <spechi0b>
        // <qint_spec>  → <spechi1b>
        // <qt_tbl>     → <qbyt_spec> s64chr()
        //              | <qint_spec> s64int()
        // <dqt_data>   → <qt_tbl>
        //              | <dqt_data> <qt_tbl>
        // <dc_spec>    → <spechi0b>
        // <ac_spec>    → <spechi1b>
        // <tabspec>    → <dc_spec>
        //              | <ac_spec>
        // <huff_tbl>   → <tabspec> huffspec()
        // <dht_data>   → <huff_tbl>
        //              | <dht_data> <huff_tbl>
        // <ul>         → <anyb>
        // <kx>         → <kxindexb>
        // <dac_cond>   → <dc_spec> <ul>
        //              | <ac_spec> <kx>
        // <dac_data>   → <dac_cond>
        //              | <dac_data> <dac_cond>
        // <rstintv>    → <intval>
        // <tblsmisc>   → ϵ 
        //              | <tblsmisc> <dqt> <len> <dqt_data>
        //              | <tblsmisc> <dht> <len> <dht_data>
        //              | <tblsmisc> <dac> <len> <dac_data>
        //              | <tblsmisc> <dri> <len> <rstintv>
        //              | <tblsmisc> <com> <len> comspec()
        //              | <tblsmisc> <appn> <len> appspec()

        /// frames, scans, and entropy-coded data
        // <prec>       → <anyb>
        // <nlines>     → <intval>
        // <linlen>     → <intval>
        // <nfc>        → <non00b>
        // <fparms>     → <len> <prec> <nlines> <linlen> <nfc> fspec()
        // <nsc>        → BY01 | BY02 | BY03 | BY04
        // <ssstart>    → <indexb>
        // <ssend>      → <indexb>
        // <sapprox>    → <anyb>
        // <prog_spec>  → <ssstart> <ssend> <sapprox>
        // <sparms>     → <len> <nsc> sspec() <prog_spec>
        // <ecdata>     → ϵ 
        //              | <ecdata> <nonFFb> 
        //              | <ecdata> <rstm>
        //              | <ecdata> BYFF BY00
        // <scan>       → <tblsmisc> <sos> <sparms> <ecdata>
        // <ecdataf>    → <ecdata>
        //              | <ecdata> <dnl> <len> <nlines>
        // <scanf>      → <tblsmisc> <sos> <sparms> <ecdataf>
        // <scanset>    → <scanf>
        //              | <scanset> <scan>
        // <nframe>     → <tblsmisc> <nsof> <fparms> <scanset>
        // <exp_data>   → BY01 | BY10 | BY11
        // <dtblsmisc>  → <tblsmisc>
        //              | <tblsmisc> <exp> BY00 BY03 <exp_data> <tblsmisc>
        // <dframe>     → <dtblsmisc> <dsof> <fparms> <scanset>

        /// compressed data stream
        // <xframes>    → ϵ 
        //              | <xframes> <dframe> 
        //              | <xframes> <nframe>
        // <fset>       → <nframe> <xframes>
        // <jpeg_data>  → <soi> <nframe> <eoi>
        //              | <soi> <tblsmisc> <eoi>
        //              | <soi> <tblsmisc> <dhp> <fparms> <fset> <eoi>
        //              | <soi> <forbid> <eoi>
    }
}
