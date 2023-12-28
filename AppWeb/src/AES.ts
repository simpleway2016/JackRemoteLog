import CryptoJS from 'crypto-js';

export class AES{
    static encrypt(plainText: string, key: string, iv: string): string {
        const keyHex = CryptoJS.enc.Utf8.parse(key.padEnd(32, 'g'));
        const ivHex = CryptoJS.enc.Utf8.parse(iv.padEnd(16,'g'));
        const encrypted = CryptoJS.AES.encrypt(plainText, keyHex, {
            iv: ivHex,
            mode: CryptoJS.mode.CBC,
            padding: CryptoJS.pad.Pkcs7
        });
        return encrypted.toString();
    }

    static decrypt(cipherText: string, key: string, iv: string): string {
        const keyHex = CryptoJS.enc.Utf8.parse(key.padEnd(32, 'g'));
        const ivHex = CryptoJS.enc.Utf8.parse(iv.padEnd(16,'g'));
        const decrypted = CryptoJS.AES.decrypt(cipherText, keyHex, {
            iv: ivHex,
            mode: CryptoJS.mode.CBC,
            padding: CryptoJS.pad.Pkcs7
        });
        return decrypted.toString(CryptoJS.enc.Utf8);
    }
}
