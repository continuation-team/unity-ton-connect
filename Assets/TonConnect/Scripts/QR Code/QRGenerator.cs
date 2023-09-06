using UnityEngine;
using System.Collections;

public class QRGenerator : MonoBehaviour {

    // Resolution
    const int PIXELS_PER_MODULE = 20;

    /// <summary>
    /// Encode text in to a QR Code
    /// </summary>
    /// <param name="text"></param>
    /// <returns></returns>
    public static Texture2D EncodeString(string text)
    {
        QRCodeGenerator qrGenerator = new QRCodeGenerator();
        QRCodeGenerator.QRCode qrCode = qrGenerator.CreateQrCode(text, QRCodeGenerator.ECCLevel.L);

        Texture2D qrTexture = qrCode.GetGraphic(PIXELS_PER_MODULE);

        return qrTexture;
    }

    /// <summary>
    /// Encode text in to a QR Code and define the colors
    /// </summary>
    /// <param name="text"></param>
    /// <param name="darkColor"></param>
    /// <param name="lightColor"></param>
    /// <returns></returns>
    public static Texture2D EncodeString(string text, Color darkColor, Color lightColor)
    {
        QRCodeGenerator qrGenerator = new QRCodeGenerator();
        QRCodeGenerator.QRCode qrCode = qrGenerator.CreateQrCode(text, QRCodeGenerator.ECCLevel.L);

        Texture2D qrTexture = qrCode.GetGraphic(PIXELS_PER_MODULE, darkColor, lightColor);

        return qrTexture;
    }

    /// <summary>
    /// Encode text in to a QR Code and define the colors and the Errer Correction Level
    /// </summary>
    /// <param name="text"></param>
    /// <param name="darkColor"></param>
    /// <param name="lightColor"></param>
    /// <param name="errorCorrectionLevel"></param>
    /// <returns></returns>
    public static Texture2D EncodeString(string text, Color darkColor, Color lightColor, QRCodeGenerator.ECCLevel errorCorrectionLevel)
    {
        QRCodeGenerator qrGenerator = new QRCodeGenerator();
        QRCodeGenerator.QRCode qrCode = qrGenerator.CreateQrCode(text, errorCorrectionLevel);

        Texture2D qrTexture = qrCode.GetGraphic(PIXELS_PER_MODULE, darkColor, lightColor);

        return qrTexture;
    }
}
