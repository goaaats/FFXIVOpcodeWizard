﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Sapphire.Common.Network;

namespace FFXIVOpcodeWizard
{ 
    class WizardProcessor
    {
        private Queue<PacketWizard> wizards = new Queue<PacketWizard>();

        public WizardProcessor()
        {
            Setup();
        }

        private void Setup()
        {
            RegisterPacketWizard("PlayerSetup", "Please enter your character name and log in.", PacketDirection.Server,
                (packet, parameters) => packet.PacketSize > 300 &&
                                        Encoding.UTF8.GetString(packet.Data).IndexOf(parameters[0]) != -1, 1);


            RegisterPacketWizard("ClientTrigger", "Please draw your weapon.", PacketDirection.Client,
                (packet, _) =>
                    packet.PacketSize == 64 && BitConverter.ToUInt32(packet.Data, (int) Offsets.IpcData) == 1);
            RegisterPacketWizard("ActorControl", string.Empty, PacketDirection.Server,
                (packet, _) => packet.PacketSize == 56 &&
                               BitConverter.ToUInt32(packet.Data, (int) Offsets.IpcData + 4) == 1);


            RegisterPacketWizard("Playtime", "Please type /playtime...", PacketDirection.Server,
                (packet, _) => packet.PacketSize == 40);


            RegisterPacketWizard("MarketBoardSearchResult", "Please click \"Catalysts\" on the market board.",
                PacketDirection.Server,
                (packet, _) => packet.PacketSize == 208 &&
                               BitConverter.ToUInt32(packet.Data, (int) Offsets.IpcData + 56) == 17837);


            RegisterPacketWizard("MarketBoardItemListingCount",
                "Please open the market board listings for Grade 7 Dark Matter...", PacketDirection.Server,
                (packet, _) => packet.PacketSize == 48 &&
                               BitConverter.ToUInt32(packet.Data, (int) Offsets.IpcData) == 17837);
            RegisterPacketWizard("MarketBoardItemListingHistory", string.Empty, PacketDirection.Server,
                (packet, _) => packet.PacketSize == 1080 &&
                               BitConverter.ToUInt32(packet.Data, (int) Offsets.IpcData) == 17837);
            RegisterPacketWizard("MarketBoardItemListing", string.Empty, PacketDirection.Server,
                (packet, _) => packet.PacketSize > 1552 &&
                               BitConverter.ToUInt32(packet.Data, (int) Offsets.IpcData + 44) == 17837);


            RegisterPacketWizard("NpcSpawn", "Scanning for NpcSpawn. Please enter your retainer name.",
                PacketDirection.Server,
                (packet, parameters) => packet.PacketSize > 592 &&
                                        Encoding.UTF8.GetString(packet.Data.Skip(592).Take(parameters[0].Length)
                                            .ToArray()) == parameters[0], 1);
            RegisterPacketWizard("PlayerSpawn", "Scanning for PlayerSpawn. Please enter your world ID.",
                PacketDirection.Server, (packet, parameters) =>
                    packet.PacketSize > 500 && BitConverter.ToUInt16(packet.Data, (int) Offsets.IpcData + 4) ==
                    int.Parse(parameters[0]), 1);


            RegisterPacketWizard("ItemInfo", "Please teleport and open your chocobo saddlebag...",
                PacketDirection.Server,
                (packet, _) => packet.PacketSize == 96 &&
                               BitConverter.ToUInt16(packet.Data, (int) Offsets.IpcData + 8) == 4000);


            RegisterPacketWizard("UpdateClassInfo",
                "Scanning for UpdateClassInfo. Please enter the level of the job you will switch to and switch to it.",
                PacketDirection.Server, (packet, parameters) =>
                    packet.PacketSize == 48 && BitConverter.ToUInt16(packet.Data, (int) Offsets.IpcData + 4) ==
                    int.Parse(parameters[0]), 1);


            RegisterPacketWizard("InitZone", "Please teleport to New Gridania.", PacketDirection.Server,
                (packet, _) => packet.PacketSize == 128 &&
                               BitConverter.ToUInt16(packet.Data, (int) Offsets.IpcData + 2) == 132);


            RegisterPacketWizard("EventStart", "Please begin fishing and put your rod away immediately",
                PacketDirection.Server,
                (packet, _) => packet.PacketSize == 56 &&
                               BitConverter.ToUInt32(packet.Data, (int) Offsets.IpcData + 8) == 0x150001);
            RegisterPacketWizard("EventPlay", string.Empty, PacketDirection.Server,
                (packet, _) => packet.PacketSize == 72 &&
                               BitConverter.ToUInt32(packet.Data, (int) Offsets.IpcData + 8) == 0x150001);
            RegisterPacketWizard("EventFinish", string.Empty, PacketDirection.Server,
                (packet, _) => packet.PacketSize == 48 &&
                               BitConverter.ToUInt32(packet.Data, (int) Offsets.IpcData) == 0x150001 &&
                               packet.Data[(int) Offsets.IpcData + 4] == 0x14 &&
                               packet.Data[(int) Offsets.IpcData + 5] == 0x01);


            RegisterPacketWizard("EventUnk0", "Please cast your line and catch a fish.", PacketDirection.Server,
                (packet, _) => packet.PacketSize == 80 &&
                               BitConverter.ToUInt32(packet.Data, (int) Offsets.IpcData + 0x1C) == 284);
            RegisterPacketWizard("EventUnk1", string.Empty, PacketDirection.Server,
                (packet, _) => packet.PacketSize == 56 &&
                               BitConverter.ToUInt32(packet.Data, (int) Offsets.IpcData + 0x08) == 257);


            RegisterPacketWizard("UseMooch", "Please catch a moochable 'Harbor Herring' from Mist using Pill Bug bait.",
                PacketDirection.Server,
                (packet, _) => packet.PacketSize == 80 &&
                               BitConverter.ToUInt32(packet.Data, (int) Offsets.IpcData + 0x18) == 2587);


            RegisterPacketWizard("CfPreferredRole", "Please wait.", PacketDirection.Server, (packet, _) =>
            {
                if (packet.PacketSize != 48)
                    return false;

                var allInRange = true;

                for (var i = 1; i < 10; i++)
                    if (packet.Data[(int) Offsets.IpcData + i] > 4 || packet.Data[(int) Offsets.IpcData + i] < 1)
                        allInRange = false;

                return allInRange;
            });
            RegisterPacketWizard("CfNotifyPop", "Please queue for \"The Vault\" as an undersized party.",
                PacketDirection.Server,
                (packet, _) => packet.PacketSize == 64 && packet.Data[(int) Offsets.IpcData + 20] == 0x22);
        }

        private void RegisterPacketWizard(string opName, string tutorial, PacketDirection scanDirection, Func<MetaPacket, string[], bool> del, int paramCount = 0)
        {
            wizards.Enqueue(new PacketWizard
            {
                OpName = opName,
                Tutorial = tutorial,
                PacketCheckerFunc = del,
                ParamCount = paramCount,
                ScanDirection = scanDirection
            });
        }

        public void Run(LinkedList<Packet> pq)
        {
            StringBuilder output = new StringBuilder();
            
            // Game Version
            Console.WriteLine("Please enter the current game version: ");
            var versionNameFilter = new Regex(@"[^0-9.]");
            var gamePatch = versionNameFilter.Replace(Console.ReadLine(), (match) => "");

            Console.WriteLine("Press enter to run all wizards or the number of a wizard to skip to it.");
            var skipCount = Console.ReadLine();

            var count = 0;

            if (!string.IsNullOrEmpty(skipCount))
            {
                count = int.Parse(skipCount);

                for (var i = 0; i < count; i++)
                    wizards.Dequeue();
            }

            while (wizards.Count > 0)
            {
                var wizard = wizards.Dequeue();

                Console.WriteLine($"#{count}: Now scanning for {wizard.OpName}");

                if (!string.IsNullOrEmpty(wizard.Tutorial))
                    Console.WriteLine(wizard.Tutorial);

                var parameters = new string[wizard.ParamCount];
                if (wizard.ParamCount > 0)
                {
                    for (var paramIndex = 0; paramIndex < wizard.ParamCount; paramIndex++)
                    {
                        Console.WriteLine($"Please now enter parameter #{paramIndex}:");
                        var thisParam = Console.ReadLine();
                        parameters[paramIndex] = thisParam;
                    }
                }
                
                Console.WriteLine($"Scanning for {wizard.ScanDirection} packets...");

                var opCode = 0;
                switch (wizard.ScanDirection)
                {
                    case PacketDirection.Server:
                        opCode = PacketProcessors.ScanInbound(pq, wizard.PacketCheckerFunc, parameters);
                        break;
                    case PacketDirection.Client:
                        opCode = PacketProcessors.ScanOutbound(pq, wizard.PacketCheckerFunc, parameters);
                        break;
                }

                Console.WriteLine($"{wizard.OpName} found at opcode 0x{opCode:x}!");
                output.Append(wizard.OpName).Append(": 0x").Append(opCode.ToString("X4")).Append(", // updated ").AppendLine(gamePatch);

                Console.WriteLine();
                count++;
            }

            // Done
            Console.WriteLine("All packets found!\n\n");
            Console.WriteLine(output.ToString());
            Console.ReadLine();
        }
    }
}
