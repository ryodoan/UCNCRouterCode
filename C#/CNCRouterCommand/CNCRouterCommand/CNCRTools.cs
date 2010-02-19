﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO.Ports;
using System.Collections;

namespace CNCRouterCommand
{
    public static class CNCRTools
    {
        public static string[] GetCommPortList()
        {
            string[] temp = SerialPort.GetPortNames();
            Array.Sort(temp);
            return temp;
        }

        public static string[] GetCNCRouterPorts()
        {
            List<string> results = new List<string>();
            string[] temp = SerialPort.GetPortNames();
            // "Ping" each port and save the ones that "Acknowledge" 
            //    w/ a supported FW version to the "results" list.

            return results.ToArray();
        }

        public static string GetCNCRouterVersion(string SerialPortName)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Creates an int16 variable from three bytes in a parity byte array.
        /// Requires parityBytes.Length - startIndex to be greater than 3.
        /// </summary>
        /// <param name="parityBytes">Array of parity bytes containing an
        ///                           Int16.</param>
        /// <param name="startIndex">Index of parityBytes to start parsing.</param>
        /// <returns>An int16 contained in parityBytes.</returns>
        public static Int16 generateInt16FromThreeBytes(byte[] parityBytes, int startIndex)
        {
            if ((parityBytes.Length - startIndex) < 3)
                throw new ArgumentOutOfRangeException("startIndex",
                    "startIndex does not allow for 3 bytes.");

            byte[] result = new byte[2];
            result[0] = Convert.ToByte(parityBytes[0] & 254);
            result[1] = Convert.ToByte(parityBytes[1] & 254);
            result[0] |= Convert.ToByte((parityBytes[2] & 4) >> 2);
            result[1] |= Convert.ToByte((parityBytes[2] & 2) >> 1);
            return BitConverter.ToInt16(result, 0);
        }

        /// <summary>
        /// Generate parity bytes from an int16 variable.
        /// </summary>
        /// <param name="value">Int16 to convert to parity bytes.</param>
        /// <returns>Parity bytes for an int16.</returns>
        public static byte[] generateThreeBytesFromInt16(Int16 value)
        {
            byte[] result = new byte[3]; // Takes 3 bytes to hold a paritized int16
            byte[] tempBytes = BitConverter.GetBytes(value);
            result[0] = Convert.ToByte(tempBytes[0] & 254); // grab top 7 bits.
            result[1] = Convert.ToByte(tempBytes[1] & 254); // Grab to 7 bits.
            // Grab the lowest bit for both bytes and place them in the final byte.
            // [0000 0 tB0_lowest tB1_Lowest Parity]
            result[2] = Convert.ToByte(((tempBytes[0] & 1) << 2) | ((tempBytes[1] & 1) << 1));
            return result;
        }

        public static void generateParity(ref byte[] serialBytes)
        {
            // Look at creating a combined ParityBits parityByte
            generateParityBits(ref serialBytes, serialBytes.Length - 1);
            generateParityByte(ref serialBytes);
        }
        /// <summary>
        /// Generates the final parity byte by counting the number of bits in
        /// each column.
        /// </summary>
        /// <param name="serialBytes">
        /// This parameter is passed by reference, the final byte in the array
        /// is used for the parity byte.  serialBytes must be at least two bytes.
        /// </param>
        public static void generateParityByte(ref byte[] serialBytes)
        {
            //TODO: generateParityByte: Look into generating both the row & column parity bits here.
            if (serialBytes.Length < 2)
                throw new ArgumentOutOfRangeException("serialBytes",
                    "serialBytes must be at least two bytes long.");

            // Set the final byte, the parity byte, to 0.
            int z = serialBytes.Length - 1;
            serialBytes[z] = 0;

            // Create a sliding binary '1' to filter for binary
            // digits in each byte.
            for (UInt16 i = 1; i <= 255; i <<= 1)
            {
                // Count the number of '1' digits in each column.
                // z is the number of actual data bytes (no parity byte)
                int numOnes = 0;
                for (int j = 0; j < z; j++)
                {
                    if ((serialBytes[j] & i) != 0)
                        ++numOnes;
                }
                // Check to see if numOnes is odd, if so ignore it (its already 0)
                //              if numOnes is even, set that bit to 1.
                if ((numOnes & 1) == 1)
                {
                    serialBytes[z] |= Convert.ToByte(i);
                }
            }
        }

        /// <summary>
        /// Takes an array of bytes and generates their parity bits.
        /// </summary>
        /// <param name="serialBytes">Array of bytes to generate parity bits for.</param>
        public static void generateParityBits(ref byte[] serialBytes, int numberBytes)
        {
            for (int i = 0; i < numberBytes; i++)
            {
                generateParityBit(ref serialBytes[i]);
            }
        }

        /// <summary>
        /// Takes an array of bytes and generates their parity bits.
        /// </summary>
        /// <param name="serialBytes">Array of bytes to generate parity bits for.</param>
        public static void generateParityBits(ref byte[] serialBytes)
        {
            generateParityBits(ref serialBytes, serialBytes.Length);
        }

        /// <summary>
        /// Counts the number of '1' bits in the passed in byte, then fills in
        /// the final parity bit.
        /// 
        /// Odd number of 1's  = 0 parity
        /// Even number of 1's = 1 parity
        /// </summary>
        /// <param name="serialByte">Byte to generate and fill parity.</param>
        public static void generateParityBit(ref byte serialByte)
        {
            int numOnes = 0;
            // Clear the lowest bit.
            serialByte = Convert.ToByte(serialByte & 254);
            // Set the temp byte.
            byte tempByte = serialByte;
            // Count the 1 bits
            while (tempByte != 0)
            {
                numOnes += tempByte & 0x1;
                tempByte >>= 1;
            }
            // Set the parity bit.
            // If numOnes is odd
            serialByte |= Convert.ToByte(numOnes & 1);
        }

        /// <summary>
        /// Checks the value of the parity bit.  Returns false if the check fails.
        /// </summary>
        /// <param name="serialByte">Serial Byte to validate.</param>
        /// <returns>True if check succeeds, false if check fails.</returns>
        public static bool validateParityBit(byte serialByte)
        {
            // Create copy of the passed in byte.
            byte tempByte = serialByte;
            // Generate the parity for the received byte.
            generateParityBit(ref tempByte);
            // Validate the generated is the same as the received.
            if (serialByte != tempByte)
                return false;   // if it is not, fail the parity check.

            return true;    // Passed parity check.
        }

        /// <summary>
        /// Checks the value of all of the parity bits in the passed in array.
        /// Array must be at least two bytes long.  Returns true if it passes
        /// the parity check.
        /// </summary>
        /// <param name="serialBytes">Array of serial data with parity bits.
        /// Must be at least two bytes long.</param>
        /// <returns>True if it passes the parity check, false if it fails.</returns>
        public static bool validateParityBytes(byte[] serialBytes)
        {
            //TODO: validateParityBytes: Validate that this works.
            // serialBytes must be at least two bytes long.
            if (serialBytes.Length < 2)
                throw new ArgumentOutOfRangeException("serialBytes",
                    "serialBytes must be at least two bytes long.");

            // Create a copy of the passed in serial data.
            byte[] tempBytes = new byte[serialBytes.Length];
            Array.Copy(serialBytes, tempBytes, serialBytes.Length);

            // Validate the parity values of each byte in the array.
            // (length - 1 because last byte is a parity byte)
            for (int i = 0; i < serialBytes.Length - 1; i++)
            {
                if (!validateParityBit(serialBytes[i])) // Check the parity of the byte.
                    return false; // Parity check failed.
            }

            // Re-generate the parity byte on the copy.
            generateParityByte(ref tempBytes);

            // Validate the generated = the received
            if (tempBytes[tempBytes.Length] != serialBytes[serialBytes.Length])
                return false; // Parity check failed.

            return true; // Parity check passed.
        }
    }
}
