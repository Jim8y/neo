// Copyright (C) 2015-2025 The Neo Project.
//
// MainService.Tools.cs file belongs to the neo project and is free
// software distributed under the MIT software license, see the
// accompanying file LICENSE in the main directory of the
// repository or http://www.opensource.org/licenses/mit-license.php
// for more details.
//
// Redistribution and use in source and binary forms with or without
// modifications are permitted.

using Neo.ConsoleService;
using Neo.Cryptography.ECC;
using Neo.Extensions;
using Neo.SmartContract;
using Neo.VM;
using Neo.Wallets;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Reflection;
using System.Text;

namespace Neo.CLI
{
    partial class MainService
    {
        /// <summary>
        /// Process "parse" command
        /// </summary>
        [ConsoleCommand("parse", Category = "Base Commands", Description = "Parse a value to its possible conversions.")]
        private void OnParseCommand(string value)
        {
            value = Base64Fixed(value);

            var parseFunctions = new Dictionary<string, Func<string, string?>>();
            var methods = GetType().GetMethods(BindingFlags.NonPublic | BindingFlags.Instance);

            foreach (var method in methods)
            {
                var attribute = method.GetCustomAttribute<ParseFunctionAttribute>();
                if (attribute != null)
                {
                    parseFunctions.Add(attribute.Description, (Func<string, string?>)Delegate.CreateDelegate(typeof(Func<string, string?>), this, method));
                }
            }

            var any = false;

            foreach (var pair in parseFunctions)
            {
                var parseMethod = pair.Value;
                var result = parseMethod(value);

                if (result != null)
                {
                    ConsoleHelper.Info("", "-----", pair.Key, "-----");
                    ConsoleHelper.Info("", result, Environment.NewLine);
                    any = true;
                }
            }

            if (!any)
            {
                ConsoleHelper.Warning($"Was not possible to convert: '{value}'");
            }
        }

        /// <summary>
        /// Read .nef file from path and print its content in base64
        /// </summary>
        [ParseFunction(".nef file path to content base64")]
        private string? NefFileToBase64(string path)
        {
            if (Path.GetExtension(path).ToLower() != ".nef") return null;
            if (!File.Exists(path)) return null;
            return Convert.ToBase64String(File.ReadAllBytes(path));
        }

        /// <summary>
        /// Little-endian to Big-endian
        /// input:  ce616f7f74617e0fc4b805583af2602a238df63f
        /// output: 0x3ff68d232a60f23a5805b8c40f7e61747f6f61ce
        /// </summary>
        [ParseFunction("Little-endian to Big-endian")]
        private string? LittleEndianToBigEndian(string hex)
        {
            try
            {
                if (!hex.IsHex()) return null;
                return "0x" + hex.HexToBytes().Reverse().ToArray().ToHexString();
            }
            catch (FormatException)
            {
                return null;
            }
        }

        /// <summary>
        /// Big-endian to Little-endian
        /// input:  0x3ff68d232a60f23a5805b8c40f7e61747f6f61ce
        /// output: ce616f7f74617e0fc4b805583af2602a238df63f
        /// </summary>
        [ParseFunction("Big-endian to Little-endian")]
        private string? BigEndianToLittleEndian(string hex)
        {
            try
            {
                var hasHexPrefix = hex.StartsWith("0x", StringComparison.InvariantCultureIgnoreCase);
                hex = hasHexPrefix ? hex[2..] : hex;
                if (!hasHexPrefix || !hex.IsHex()) return null;
                return hex.HexToBytes().Reverse().ToArray().ToHexString();
            }
            catch
            {
                return null;
            }
        }

        /// <summary>
        /// String to Base64
        /// input:  Hello World!
        /// output: SGVsbG8gV29ybGQh
        /// </summary>
        [ParseFunction("String to Base64")]
        private string? StringToBase64(string value)
        {
            try
            {
                var bytes = value.ToStrictUtf8Bytes();
                return Convert.ToBase64String(bytes.AsSpan());
            }
            catch
            {
                return null;
            }
        }

        /// <summary>
        /// Big Integer to Base64
        /// input:  123456
        /// output: QOIB
        /// </summary>
        [ParseFunction("Big Integer to Base64")]
        private string? NumberToBase64(string value)
        {
            try
            {
                if (!BigInteger.TryParse(value, out var number)) return null;

                var bytes = number.ToByteArray();
                return Convert.ToBase64String(bytes.AsSpan());
            }
            catch
            {
                return null;
            }
        }

        /// <summary>
        /// Fix for Base64 strings containing unicode
        /// input:  DCECbzTesnBofh/Xng1SofChKkBC7jhVmLxCN1vk\u002B49xa2pBVuezJw==
        /// output: DCECbzTesnBofh/Xng1SofChKkBC7jhVmLxCN1vk+49xa2pBVuezJw==
        /// </summary>
        /// <param name="str">Base64 strings containing unicode</param>
        /// <returns>Correct Base64 string</returns>
        private static string Base64Fixed(string str)
        {
            var sb = new StringBuilder();
            for (var i = 0; i < str.Length; i++)
            {
                if (str[i] == '\\' && i + 5 < str.Length && str[i + 1] == 'u')
                {
                    var hex = str.Substring(i + 2, 4);
                    if (hex.IsHex())
                    {
                        var bts = new byte[2];
                        bts[0] = (byte)int.Parse(hex.Substring(2, 2), NumberStyles.HexNumber);
                        bts[1] = (byte)int.Parse(hex.Substring(0, 2), NumberStyles.HexNumber);
                        sb.Append(Encoding.Unicode.GetString(bts));
                        i += 5;
                    }
                    else
                    {
                        sb.Append(str[i]);
                    }
                }
                else
                {
                    sb.Append(str[i]);
                }
            }
            return sb.ToString();
        }

        /// <summary>
        /// Address to ScriptHash (big-endian)
        /// input:  NejD7DJWzD48ZG4gXKDVZt3QLf1fpNe1PF
        /// output: 0x3ff68d232a60f23a5805b8c40f7e61747f6f61ce
        /// </summary>
        [ParseFunction("Address to ScriptHash (big-endian)")]
        private string? AddressToScripthash(string address)
        {
            try
            {
                var bigEndScript = address.ToScriptHash(NeoSystem.Settings.AddressVersion);
                return bigEndScript.ToString();
            }
            catch
            {
                return null;
            }
        }

        /// <summary>
        /// Address to ScriptHash (blittleig-endian)
        /// input:  NejD7DJWzD48ZG4gXKDVZt3QLf1fpNe1PF
        /// output: ce616f7f74617e0fc4b805583af2602a238df63f
        /// </summary>
        [ParseFunction("Address to ScriptHash (little-endian)")]
        private string? AddressToScripthashLE(string address)
        {
            try
            {
                var bigEndScript = address.ToScriptHash(NeoSystem.Settings.AddressVersion);
                return bigEndScript.ToArray().ToHexString();
            }
            catch
            {
                return null;
            }
        }

        /// <summary>
        /// Address to Base64
        /// input:  NejD7DJWzD48ZG4gXKDVZt3QLf1fpNe1PF
        /// output: zmFvf3Rhfg/EuAVYOvJgKiON9j8=
        /// </summary>
        [ParseFunction("Address to Base64")]
        private string? AddressToBase64(string address)
        {
            try
            {
                var script = address.ToScriptHash(NeoSystem.Settings.AddressVersion);
                return Convert.ToBase64String(script.ToArray().AsSpan());
            }
            catch
            {
                return null;
            }
        }

        /// <summary>
        /// ScriptHash to Address
        /// input:  0x3ff68d232a60f23a5805b8c40f7e61747f6f61ce
        /// output: NejD7DJWzD48ZG4gXKDVZt3QLf1fpNe1PF
        /// </summary>
        [ParseFunction("ScriptHash to Address")]
        private string? ScripthashToAddress(string script)
        {
            try
            {
                UInt160 scriptHash;
                if (script.StartsWith("0x"))
                {
                    if (!UInt160.TryParse(script, out scriptHash))
                    {
                        return null;
                    }
                }
                else
                {
                    if (!UInt160.TryParse(script, out UInt160 littleEndScript))
                    {
                        return null;
                    }
                    var bigEndScript = littleEndScript.ToArray().ToHexString();
                    if (!UInt160.TryParse(bigEndScript, out scriptHash))
                    {
                        return null;
                    }
                }

                return scriptHash.ToAddress(NeoSystem.Settings.AddressVersion);
            }
            catch
            {
                return null;
            }
        }

        /// <summary>
        /// Base64 to Address
        /// input:  zmFvf3Rhfg/EuAVYOvJgKiON9j8=
        /// output: NejD7DJWzD48ZG4gXKDVZt3QLf1fpNe1PF
        /// </summary>
        [ParseFunction("Base64 to Address")]
        private string? Base64ToAddress(string bytearray)
        {
            try
            {
                var result = Convert.FromBase64String(bytearray).Reverse().ToArray();
                var hex = result.ToHexString();

                if (!UInt160.TryParse(hex, out var scripthash))
                {
                    return null;
                }

                return scripthash.ToAddress(NeoSystem.Settings.AddressVersion);
            }
            catch
            {
                return null;
            }
        }

        /// <summary>
        /// Base64 to String
        /// input:  SGVsbG8gV29ybGQh
        /// output: Hello World!
        /// </summary>
        [ParseFunction("Base64 to String")]
        private string? Base64ToString(string bytearray)
        {
            try
            {
                var result = Convert.FromBase64String(bytearray);
                var utf8String = result.ToStrictUtf8String();
                return IsPrintable(utf8String) ? utf8String : null;
            }
            catch
            {
                return null;
            }
        }

        /// <summary>
        /// Base64 to Big Integer
        /// input:  QOIB
        /// output: 123456
        /// </summary>
        [ParseFunction("Base64 to Big Integer")]
        private string? Base64ToNumber(string bytearray)
        {
            try
            {
                var bytes = Convert.FromBase64String(bytearray);
                var number = new BigInteger(bytes);
                return number.ToString();
            }
            catch
            {
                return null;
            }
        }

        /// <summary>
        /// Public Key to Address
        /// input:  03dab84c1243ec01ab2500e1a8c7a1546a26d734628180b0cf64e72bf776536997
        /// output: NU7RJrzNgCSnoPLxmcY7C72fULkpaGiSpJ
        /// </summary>
        [ParseFunction("Public Key to Address")]
        private string? PublicKeyToAddress(string pubKey)
        {
            if (ECPoint.TryParse(pubKey, ECCurve.Secp256r1, out var publicKey) == false)
                return null;
            return Contract.CreateSignatureContract(publicKey)
                .ScriptHash
                .ToAddress(NeoSystem.Settings.AddressVersion);
        }

        /// <summary>
        /// WIF to Public Key
        /// </summary>
        [ParseFunction("WIF to Public Key")]
        private string? WIFToPublicKey(string wif)
        {
            try
            {
                var privateKey = Wallet.GetPrivateKeyFromWIF(wif);
                var account = new KeyPair(privateKey);
                return account.PublicKey.ToArray().ToHexString();
            }
            catch (Exception)
            {
                return null;
            }
        }

        /// <summary>
        /// WIF to Address
        /// </summary>
        [ParseFunction("WIF to Address")]
        private string? WIFToAddress(string wif)
        {
            try
            {
                var pubKey = WIFToPublicKey(wif);
                if (string.IsNullOrEmpty(pubKey)) return null;

                return Contract.CreateSignatureContract(ECPoint.Parse(pubKey, ECCurve.Secp256r1)).ScriptHash.ToAddress(NeoSystem.Settings.AddressVersion);
            }
            catch (Exception)
            {
                return null;
            }
        }

        /// <summary>
        /// Base64 Smart Contract Script Analysis
        /// input: DARkYXRhAgBlzR0MFPdcrAXPVptVduMEs2lf1jQjxKIKDBT3XKwFz1abVXbjBLNpX9Y0I8SiChTAHwwIdHJhbnNmZXIMFKNSbimM12LkFYX/8KGvm2ttFxulQWJ9W1I=
        /// output:
        /// PUSHDATA1 data
        /// PUSHINT32 500000000
        /// PUSHDATA1 0x0aa2c42334d65f69b304e376559b56cf05ac5cf7
        /// PUSHDATA1 0x0aa2c42334d65f69b304e376559b56cf05ac5cf7
        /// PUSH4
        /// PACK
        /// PUSH15
        /// PUSHDATA1 transfer
        /// PUSHDATA1 0xa51b176d6b9bafa1f0ff8515e462d78c296e52a3
        /// SYSCALL System.Contract.Call
        /// </summary>
        [ParseFunction("Base64 Smart Contract Script Analysis")]
        private string? ScriptsToOpCode(string base64)
        {
            try
            {
                var bytes = Convert.FromBase64String(base64);
                var sb = new StringBuilder();
                var line = 0;

                foreach (var instruct in new VMInstruction(bytes))
                {
                    if (instruct.OperandSize == 0)
                        sb.AppendFormat("L{0:D04}:{1:X04} {2}{3}", line, instruct.Position, instruct.OpCode, Environment.NewLine);
                    else
                        sb.AppendFormat("L{0:D04}:{1:X04} {2,-10}{3}{4}", line, instruct.Position, instruct.OpCode, instruct.DecodeOperand(), Environment.NewLine);
                    line++;
                }

                return sb.ToString();
            }
            catch
            {
                return null;
            }
        }

        /// <summary>
        /// Base64 .nef file Analysis
        /// </summary>
        [ParseFunction("Base64 .nef file Analysis")]
        private string? NefFileAnalyis(string base64)
        {
            byte[] nefData;
            if (File.Exists(base64))  // extension name not considered
                nefData = File.ReadAllBytes(base64);
            else
            {
                try
                {
                    nefData = Convert.FromBase64String(base64);
                }
                catch { return null; }
            }
            NefFile nef;
            Script script;
            bool verifyChecksum = false;
            bool strictMode = false;
            try
            {
                nef = NefFile.Parse(nefData, true);
                verifyChecksum = true;
            }
            catch (FormatException)
            {
                nef = NefFile.Parse(nefData, false);
            }
            catch { return null; }
            try
            {
                script = new Script(nef.Script, true);
                strictMode = true;
            }
            catch (BadScriptException)
            {
                script = new Script(nef.Script, false);
            }
            catch { return null; }
            string? result = ScriptsToOpCode(Convert.ToBase64String(nef.Script.ToArray()));
            if (result == null)
                return null;
            string prefix = $"\r\n# Compiler: {nef.Compiler}";
            if (!verifyChecksum)
                prefix += $"\r\n# Warning: Invalid .nef file checksum";
            if (!strictMode)
                prefix += $"\r\n# Warning: Failed in {nameof(strictMode)}";
            return prefix + result;
        }

        /// <summary>
        /// Checks if the string is null or cannot be printed.
        /// </summary>
        /// <param name="value">
        /// The string to test
        /// </param>
        /// <returns>
        /// Returns false if the string is null, or if it is empty, or if each character cannot be printed;
        /// otherwise, returns true.
        /// </returns>
        private static bool IsPrintable(string value)
        {
            return !string.IsNullOrWhiteSpace(value) && value.Any(c => !char.IsControl(c));
        }
    }
}
