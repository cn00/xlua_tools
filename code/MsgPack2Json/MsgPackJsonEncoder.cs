
using System;
using System.IO;

namespace Comm
{
    public class MsgPackJsonEncoder
    {
        public static void Main(string[] args)
        {
            var fpath = args[0];
            var json = MsgPackToJson(File.ReadAllBytes(fpath));
            Console.WriteLine(json);
        }

        public static string
        MsgPackToJson( byte[] msgPackBin )
        {
            if( msgPackBin == null || msgPackBin.Length < 1 )
            {
                return string.Empty;
            }

            int len = msgPackBin.Length;
            var sb = new System.Text.StringBuilder( 1024 );
            parse( msgPackBin, 0, sb );

            return sb.ToString();
        }

        private static int
        parse( byte[] msgPackBin, int currentIndex, System.Text.StringBuilder sb )
        {
            
            byte tar = msgPackBin[ currentIndex++ ];

            if (IsPositiveFixint(tar) )
            {
                sb.Append( (int)tar );
            }
            else
            if( IsFixmap(tar) )
            {
                sb.Append( "{" );
                int fixmapLength = (int)(tar & 0x0f);
                for( int i=0; i<fixmapLength; i++ )
                {
                    if( i > 0 )
                    {
                        sb.Append( "," );
                    }
                    currentIndex = parse( msgPackBin, currentIndex, sb );
                    sb.Append( ":" );
                    currentIndex = parse( msgPackBin, currentIndex, sb );
                }
                sb.Append( "}" );
            }
            else
            if( IsFixarray(  tar ) )
            {
                sb.Append( "[" );
                int fixarrayLength = (int)(tar & 0x0f);
                for (int i = 0; i < fixarrayLength; i++)
                {
                    if (i > 0)
                    {
                        sb.Append( "," );
                    }
                    currentIndex = parse( msgPackBin, currentIndex, sb );
                }
                sb.Append( "]" );
            }
            else
            if( IsFixstr (  tar ) )
            {
                int fixstrLength = (int)(tar & 0x1f);
                sb.Append( "\"");
                string text = System.Text.Encoding.UTF8.GetString( msgPackBin, currentIndex, fixstrLength )
	                .Replace("\n", "\\n")
	                .Replace("\r", "\\r");
                sb.Append( text );
                currentIndex += fixstrLength;
                sb.Append( "\"" );
            }
            else
            if( IsNil( tar ))
            {
                //sb.Append( "nil" );
                sb.Append( "null" );
            }
            else
            if( IsFalse( tar ))
            {
                sb.Append( "false" );
            }
            else
            if( IsTrue( tar ))
            {
                sb.Append( "true" );
            }
            else
            if( IsBin8( tar ))
            {
                int size = (int)msgPackBin[ currentIndex++ ];
                for( int i=0; i<size; i++ )
                {
                    sb.Append( msgPackBin[currentIndex++] );
                }
            }
            else
            if( IsBin16(  tar )  )
            {
                int size = (int)(msgPackBin[currentIndex++] << 8 |
                                 msgPackBin[currentIndex++]);
                for (int i = 0; i < size; i++)
                {
                    sb.Append( msgPackBin[currentIndex++] );
                }
            }
            else
            if(IsBin32(  tar )  )
            {
                int size = (int)(msgPackBin[currentIndex++] << 24 |
                                 msgPackBin[currentIndex++] << 16 |
                                 msgPackBin[currentIndex++] << 8 |
                                 msgPackBin[currentIndex++]);
                for (int i = 0; i < size; i++)
                {
                    sb.Append( msgPackBin[currentIndex++] );
                }
            }
            else
            if( IsExt8( tar) )
            {
                // Skip.
                int size = (int)msgPackBin[currentIndex++];
                currentIndex++; // type
                currentIndex += size;   // data
            }
            else
            if( IsExt16( tar) )
            {
                // Skip.
                int size = ((int)msgPackBin[currentIndex++] << 8) + (int)msgPackBin[currentIndex++];
                currentIndex++; // type
                currentIndex += size;   // data
            }
            else
            if( IsExt32( tar) )
            {
                // Skip.
                int size = ((int)msgPackBin[currentIndex++] << 24) +
                           ((int)msgPackBin[currentIndex++] << 16) +
                           ((int)msgPackBin[currentIndex++] << 8) +
                           (int)msgPackBin[currentIndex++];
                currentIndex++; // type
                currentIndex += size;   // data
            }
            else
            if(IsFloat32( tar )  )
            {
                float f = (float)((msgPackBin[currentIndex++] << 24) |
                                  (msgPackBin[currentIndex++] << 16) |
                                  (msgPackBin[currentIndex++] << 8) |
                                   msgPackBin[currentIndex++]);
                sb.Append( f );
            }
            else
            if(IsFloat64( tar )  )
            {
                double f = (double)((msgPackBin[currentIndex++] << 56) |
                                  (msgPackBin[currentIndex++] << 48) |
                                  (msgPackBin[currentIndex++] << 40) |
                                  (msgPackBin[currentIndex++] << 32) |
                                  (msgPackBin[currentIndex++] << 24) |
                                  (msgPackBin[currentIndex++] << 16) |
                                  (msgPackBin[currentIndex++] << 8) |
                                   msgPackBin[currentIndex++]);
                sb.Append( f );
            }
            else
            if(IsUint8( tar )  )
            {
                byte u = (byte)msgPackBin[currentIndex++];
                sb.Append( u );
            }
            else
            if( IsUint16( tar) )
            {
                ushort u = (ushort)(msgPackBin[currentIndex++] << 8 |
                                msgPackBin[currentIndex++]);
                sb.Append( u );
            }
            else
            if( IsUint32( tar)  )
            {
                uint u = (uint)(msgPackBin[currentIndex++] << 24 |
                                msgPackBin[currentIndex++] << 16 |
                                msgPackBin[currentIndex++] << 8 |
                                msgPackBin[currentIndex++]);
                sb.Append( u );
            }
            else
            if( IsUint64( tar) )
            {
                var tar56 = (ulong)((ulong)msgPackBin[currentIndex++] << 56);
                var tar48 = (ulong)((ulong)msgPackBin[currentIndex++] << 48);
                var tar40 = (ulong)((ulong)msgPackBin[currentIndex++] << 40);
                var tar32 = (ulong)((ulong)msgPackBin[currentIndex++] << 32);
                var tar24 = (ulong)(msgPackBin[currentIndex++] << 24);
                var tar16 = (ulong)(msgPackBin[currentIndex++] << 16);
                var tar8 = (ulong)(msgPackBin[currentIndex++] << 8);
                var tar0 = (ulong)(msgPackBin[currentIndex++]);

                ulong u = (tar56 | tar48 | tar40 | tar32 | tar24 | tar16 | tar8 | tar0);
                sb.Append( u );
            }
            else
            if( IsInt8( tar) )
            {
                sbyte i = (sbyte)msgPackBin[currentIndex++];
                sb.Append( i );
            }
            else
            if(IsInt16( tar)  )
            {
                short i = (short)(msgPackBin[currentIndex++] << 8 |
                              msgPackBin[currentIndex++]);
                sb.Append( i );
            }
            else
            if( IsInt32( tar)  )
            {
                int u = (int)(msgPackBin[currentIndex++] << 24 |
                              msgPackBin[currentIndex++] << 16 |
                              msgPackBin[currentIndex++] << 8 |
                              msgPackBin[currentIndex++]);
                sb.Append( u );
            }
            else
            if( IsInt64( tar)  )
            {
                var tar56 = (long)((long)msgPackBin[currentIndex++] << 56);
                var tar48 = (long)((long)msgPackBin[currentIndex++] << 48);
                var tar40 = (long)((long)msgPackBin[currentIndex++] << 40);
                var tar32 = (long)((long)msgPackBin[currentIndex++] << 32);
                var tar24 = (long)(msgPackBin[currentIndex++] << 24);
                var tar16 = (long)(msgPackBin[currentIndex++] << 16);
                var tar8 = (long)(msgPackBin[currentIndex++] << 8);
                var tar0 = (long)(msgPackBin[currentIndex++]);

                long u = (tar56 | tar48 | tar40 | tar32 | tar24 | tar16 | tar8 | tar0);
                sb.Append( u );
            }
            else
            if(IsFixext1( tar )  )
            {
                // Skip.
                currentIndex += 3;
            }
            else
            if(   IsFixext2( tar) )
            {
                // Skip.
                currentIndex += 4;
            }
            else
            if(  IsFixext4( tar) )
            {
                // Skip.
                currentIndex += 6;
            }
            else
            if( IsFixext8( tar) )
            {
                // Skip.
                currentIndex += 10;
            }
            else
            if( IsFixext16( tar)  )
            {
                // Skip.
                currentIndex += 18;
            }
            else
            if(IsStr8( tar )  )
            {
                int size = (int)msgPackBin[currentIndex++];
                string text = System.Text.Encoding.UTF8.GetString( msgPackBin, currentIndex, size )
	                .Replace("\n", "\\n")
	                .Replace("\r", "\\r");
                sb.Append( "\"" );
                sb.Append( text );
                sb.Append( "\"" );
                currentIndex += size;
            }
            else
            if(IsStr16( tar)  )
            {
                int size = (int)(msgPackBin[currentIndex++] << 8 |
                                 msgPackBin[currentIndex++]);
                string text = System.Text.Encoding.UTF8.GetString( msgPackBin, currentIndex, size )
	                .Replace("\n", "\\n")
	                .Replace("\r", "\\r");
                sb.Append( "\"" );
                sb.Append( text );
                sb.Append( "\"" );
                currentIndex += size;
            }
            else
            if(IsStr32( tar) )
            {
                int size = (int)(msgPackBin[currentIndex++] << 24 |
                                 msgPackBin[currentIndex++] << 16 |
                                 msgPackBin[currentIndex++] << 8 |
                                 msgPackBin[currentIndex++]);
                string text = System.Text.Encoding.UTF8.GetString( msgPackBin, currentIndex, size )
	                .Replace("\n", "\\n")
	                .Replace("\r", "\\r");
                sb.Append( "\"" );
                sb.Append( text );
                sb.Append( "\"" );
                currentIndex += size;
            }
            else
            if( IsArray16( tar ) )
            {
                sb.Append( "[" );
                int size = (int)(msgPackBin[currentIndex++] << 8 |
                                 msgPackBin[currentIndex++]);
                for (int i = 0; i < size; i++)
                {
                    if (i > 0)
                    {
                        sb.Append( "," );
                    }
                    currentIndex = parse( msgPackBin, currentIndex, sb );
                }
                sb.Append( "]" );
            }
            else
            if( IsArray32( tar) )
            {
                sb.Append( "[" );
                int size = (int)(msgPackBin[currentIndex++] << 24 |
                                 msgPackBin[currentIndex++] << 16 |
                                 msgPackBin[currentIndex++] << 8 |
                                 msgPackBin[currentIndex++]);
                for (int i = 0; i < size; i++)
                {
                    if (i > 0)
                    {
                        sb.Append( "," );
                    }
                    currentIndex = parse( msgPackBin, currentIndex, sb );
                }
                sb.Append( "]" );
            }
            else
            if(  IsMap16( tar) )
            {
                sb.Append( "{" );
                int size = (int)(msgPackBin[currentIndex++] << 8 |
                                 msgPackBin[currentIndex++]);
                for (int i = 0; i < size; i++)
                {
                    if (i > 0)
                    {
                        sb.Append( "," );
                    }
                    currentIndex = parse( msgPackBin, currentIndex, sb );
                    sb.Append( ":" );
                    currentIndex = parse( msgPackBin, currentIndex, sb );
                }
                sb.Append( "}" );
            }
            else
            if( IsMap32( tar) )
            {
                sb.Append( "{" );
                int size = (int)(msgPackBin[currentIndex++] << 24 |
                                 msgPackBin[currentIndex++] << 16 |
                                 msgPackBin[currentIndex++] << 8 |
                                 msgPackBin[currentIndex++]);
                for (int i = 0; i < size; i++)
                {
                    if (i > 0)
                    {
                        sb.Append( "," );
                    }
                    currentIndex = parse( msgPackBin, currentIndex, sb );
                    sb.Append( ":" );
                    currentIndex = parse( msgPackBin, currentIndex, sb );
                }
                sb.Append( "}" );
            }
            else
            if(  IsNegativeFixint( tar ) )
            {
                sb.Append( (int)tar );
            }

            return currentIndex;
        }


        public static bool
        IsPositiveFixint(byte tar)
        {
            return (tar >= 0x00 && tar <= 0x7f);
        }

        public static bool
        IsFixmap(byte tar)
        {
            return (tar >= 0x80 && tar <= 0x8f);
        }

        public static bool
        IsFixarray(byte tar)
        {
            return (tar >= 0x90 && tar <= 0x9f);
        }

        public static bool
        IsFixstr(byte tar)
        {
            return (tar >= 0xa0 && tar <= 0xbf);
        }

        public static bool
        IsNil(byte tar)
        {
            return tar == 0xc0;
        }

        public static bool
        IsFalse(byte tar)
        {
            return tar == 0xc2;
        }

        public static bool
        IsTrue(byte tar)
        {
            return tar == 0xc3;
        }

        public static bool
        IsBin8(byte tar)
        {
            return tar == 0xc4;
        }

        public static bool
        IsBin16(byte tar)
        {
            return tar == 0xc5;
        }

        public static bool
        IsBin32(byte tar)
        {
            return tar == 0xc6;
        }

        public static bool
        IsExt8(byte tar)
        {
            return tar == 0xc7;
        }

        public static bool
        IsExt16(byte tar)
        {
            return tar == 0xc8;
        }

        public static bool
        IsExt32(byte tar)
        {
            return tar == 0xc9;
        }

        public static bool
        IsFloat32(byte tar)
        {
            return tar == 0xca;
        }

        public static bool
        IsFloat64(byte tar)
        {
            return tar == 0xcb;
        }

        public static bool
        IsUint8(byte tar)
        {
            return tar == 0xcc;
        }

        public static bool
        IsUint16(byte tar)
        {
            return tar == 0xcd;
        }

        public static bool
        IsUint32(byte tar)
        {
            return tar == 0xce;
        }

        public static bool
        IsUint64(byte tar)
        {
            return tar == 0xcf;
        }

        public static bool
        IsInt8(byte tar)
        {
            return tar == 0xd0;
        }

        public static bool
        IsInt16(byte tar)
        {
            return tar == 0xd1;
        }

        public static bool
        IsInt32(byte tar)
        {
            return tar == 0xd2;
        }

        public static bool
        IsInt64(byte tar)
        {
            return tar == 0xd3;
        }

        public static bool
        IsFixext1(byte tar)
        {
            return tar == 0xd4;
        }

        public static bool
        IsFixext2(byte tar)
        {
            return tar == 0xd5;
        }

        public static bool
        IsFixext4(byte tar)
        {
            return tar == 0xd6;
        }

        public static bool
        IsFixext8(byte tar)
        {
            return tar == 0xd7;
        }

        public static bool
        IsFixext16(byte tar)
        {
            return tar == 0xd8;
        }

        public static bool
        IsStr8(byte tar)
        {
            return tar == 0xd9;
        }

        public static bool
        IsStr16(byte tar)
        {
            return tar == 0xda;
        }

        public static bool
        IsStr32(byte tar)
        {
            return tar == 0xdb;
        }

        public static bool
        IsArray16(byte tar)
        {
            return tar == 0xdc;
        }

        public static bool
        IsArray32(byte tar)
        {
            return tar == 0xdd;
        }

        public static bool
        IsMap16(byte tar)
        {
            return tar == 0xde;
        }

        public static bool
        IsMap32(byte tar)
        {
            return tar == 0xdf;
        }

        public static bool
        IsNegativeFixint(byte tar)
        {
            return (tar >= 0xe0 && tar <= 0xff);
        }


        /*
        IsPositiveFixint(byte tar)
        IsFixmap( byte tar )
        IsFixarray( byte tar )
        IsFixstr ( byte tar )
        IsNil( byte tar )
        IsFalse( byte tar )
        IsTrue( byte tar )
        IsBin8( byte tar )
        IsBin16( byte tar )
        IsBin32( byte tar )
        IsExt8(byte tar)
        IsExt16(byte tar)
        IsExt32(byte tar)
        IsFloat32(byte tar )
        IsFloat64(byte tar )
        IsUint8(byte tar )
        IsUint16(byte tar)
        IsUint32(byte tar)
        IsUint64(byte tar)
        IsInt8(byte tar)
        IsInt16(byte tar)l
        IsInt32(byte tar)
        IsInt64(byte tar)
        IsFixext1(byte tar )
        IsFixext2(byte tar)
        IsFixext4(byte tar)
        IsFixext8(byte tar)
        IsFixext16(byte tar)
        IsStr8(byte tar )
        IsStr16(byte tar)
        IsStr32(byte tar)
        IsArray16(byte tar )
        IsArray32(byte tar)
        IsMap16(byte tar)
        IsMap32(byte tar)
        IsNegativeFixint(byte tar )
         */
    }
}
