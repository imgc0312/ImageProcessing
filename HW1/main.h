
struct BMPHeader{
    //header
    unsigned short int type = 0x4D42;         /* Magic identifier            */
    unsigned int size = 0;                  /* File size in bytes          */
    unsigned short int reserved1 = 0;
    unsigned short int reserved2 = 0;
    unsigned int offset = 54;               /* Offset to image data, bytes */
    //info
    unsigned int size 40;                   /* Header info size in bytes      */
    int width = 0;                          /* Width and height of image */
    int height = 0;                         /* Width and height of image */
    unsigned short int planes = 1;          /* Number of colour planes   */
    unsigned short int bits = 24;           /* Bits per pixel            */
    unsigned int compression = 0;           /* Compression type          */
    unsigned int imagesize = 0;             /* Image size in bytes       */
    int xresolution = 0;                    /* Pixels per meter          */
    int yresolution = 0;                    /* Pixels per meter          */
    unsigned int ncolours = 0;              /* Number of colours         */
    unsigned int importantcolours = 0;      /* Important colours         */
};

struct PCXHeader{

};
