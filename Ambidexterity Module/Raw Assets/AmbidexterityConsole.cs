using System;
using System.Globalization;
using UnityEngine;
using Wenzil.Console;

namespace AmbidexterityModule
{
    public class AmbidexterityConsole
    {
        const string noInstanceMessage = "FPS AmbidexterityConsole instance not found.";

        public static void RegisterCommands()
        {
            try
            {
                ConsoleCommandsDatabase.RegisterCommand(OffsetDistance.name, OffsetDistance.description, OffsetDistance.usage, OffsetDistance.Execute);
                ConsoleCommandsDatabase.RegisterCommand(DisableSmoothAnimations.name, DisableSmoothAnimations.description, DisableSmoothAnimations.usage, DisableSmoothAnimations.Execute);
                ConsoleCommandsDatabase.RegisterCommand(ChangeAttackSpeed.name, ChangeAttackSpeed.description, ChangeAttackSpeed.usage, ChangeAttackSpeed.Execute);
                ConsoleCommandsDatabase.RegisterCommand(ChangeWeaponIndex.name, ChangeWeaponIndex.description, ChangeWeaponIndex.usage, ChangeWeaponIndex.Execute);
                ConsoleCommandsDatabase.RegisterCommand(ChangeFramePos.name, ChangeFramePos.description, ChangeFramePos.usage, ChangeFramePos.Execute);
                ConsoleCommandsDatabase.RegisterCommand(ChangeFrameSpeed.name, ChangeFrameSpeed.description, ChangeFrameSpeed.usage, ChangeFrameSpeed.Execute);
            }
            catch (Exception e)
            {
                Debug.LogError(string.Format("Error Registering RealGrass Console commands: {0}", e.Message));
            }
        }

        public class OffsetDistance
        {
            public static readonly string name = "OffsetDistance";
            public static readonly string error = "Failed to set OffsetDistance - invalid setting or DaggerfallUnity singleton object";
            public static readonly string description = "Changes animation offset distance";
            public static readonly string usage = "OffsetDistance";

            public static float offsetDistance { get; private set; }

            public static string Execute(params string[] args)
            {
                float lrange;
                DaggerfallWorkshop.DaggerfallUnity daggerfallUnity = DaggerfallWorkshop.DaggerfallUnity.Instance;

                if (daggerfallUnity == null)
                    return error;

                if (args == null || args.Length < 1)
                {
                    return "Insert a number";
                }
                else if (!float.TryParse(args[0], out lrange))
                    return error;
                else if (lrange < 0)
                    return "Improper number";
                else
                {
                    try
                    {
                        offsetDistance = lrange;
                        return string.Format("Lerp set to: {0}", lrange);
                    }
                    catch
                    {
                        return "Unspecified error; failed to set lerp";
                    }

                }
            }
        }

        public class DisableSmoothAnimations
        {
            public static readonly string name = "DisableSmoothAnimations";
            public static readonly string error = "Failed to set DisableSmoothAnimations - invalid setting or DaggerfallUnity singleton object";
            public static readonly string description = "Enables or disables smooth animations";
            public static readonly string usage = "DisableSmoothAnimations";

            public static bool disableSmoothAnimations { get; private set; }

            public static string Execute(params string[] args)
            {
                bool trigger;
                DaggerfallWorkshop.DaggerfallUnity daggerfallUnity = DaggerfallWorkshop.DaggerfallUnity.Instance;

                if (daggerfallUnity == null)
                    return error;

                if (args == null || args.Length < 1)
                {
                    return "true or false";
                }
                else if (!bool.TryParse(args[0], out trigger))
                    return error;
                else
                {
                    try
                    {
                        disableSmoothAnimations = trigger;
                        return string.Format("trigger set to:" + trigger.ToString());
                    }
                    catch
                    {
                        return "Unspecified error; failed to set lerp";
                    }

                }
            }
        }

        public class ChangeAttackSpeed
        {
            public static readonly string name = "ChangeAttackSpeed";
            public static readonly string error = "Failed to set ChangeAttackSpeed - invalid setting or DaggerfallUnity singleton object";
            public static readonly string description = "Changed AttackSpeed";
            public static readonly string usage = "ChangeAttackSpeed";

            public static float changeAttackSpeed { get; private set;}

            public static string Execute(params string[] args)
            {
                float AttackSpeed;
                DaggerfallWorkshop.DaggerfallUnity daggerfallUnity = DaggerfallWorkshop.DaggerfallUnity.Instance;

                if (daggerfallUnity == null)
                    return error;

                if (args == null || args.Length < 1)
                {
                    return "true or false";
                }
                else if (!float.TryParse(args[0], out AttackSpeed))
                    return error;
                else
                {
                    try
                    {
                        changeAttackSpeed = AttackSpeed;
                        return string.Format("trigger set to:" + AttackSpeed.ToString());
                    }
                    catch
                    {
                        return "Unspecified error; failed to set lerp";
                    }

                }
            }
        }

        public class ChangeWeaponIndex
        {
            public static readonly string name = "WeaponIndex";
            public static readonly string error = "Failed to set WeaponIndex - invalid setting or DaggerfallUnity singleton object";
            public static readonly string description = "Changed the weapon index";
            public static readonly string usage = "WeaponIndex";

            public static int changeWeaponIndex { get; private set; }

            public static string Execute(params string[] args)
            {
                int WeaponIndex;
                DaggerfallWorkshop.DaggerfallUnity daggerfallUnity = DaggerfallWorkshop.DaggerfallUnity.Instance;

                if (daggerfallUnity == null)
                    return error;

                if (args == null || args.Length < 1)
                {
                    return "true or false";
                }
                else if (!int.TryParse(args[0], out WeaponIndex))
                    return error;
                else
                {
                    try
                    {
                        changeWeaponIndex = WeaponIndex;
                        return string.Format("lerpValue set to:" + WeaponIndex.ToString());
                    }
                    catch
                    {
                        return "Unspecified error; failed to set lerp";
                    }

                }
            }
        }

        public class ChangeFramePos
        {
            public static readonly string name = "ChangeMovementMods";
            public static readonly string error = "Failed to set ChangeMovementMods - invalid setting or DaggerfallUnity singleton object";
            public static readonly string description = "Change sheathed and attack movement modifiers";
            public static readonly string usage = "ChangeMovementMods";

            public static float EPos1 { get; private set;}
            public static float EPos2 { get; private set;}
            public static float EPos3 { get; private set;}
            public static float EPos4 { get; private set;}
            public static float EPos5 { get; private set;}

            public static string Execute(params string[] args)
            {
                float Pos1;
                float Pos2;
                float Pos3;
                float Pos4;
                float Pos5;
                DaggerfallWorkshop.DaggerfallUnity daggerfallUnity = DaggerfallWorkshop.DaggerfallUnity.Instance;

                if (daggerfallUnity == null)
                    return error;

                if (args == null || args.Length < 1)
                {
                    return "-Pos1 value -Pos2 value -Pos3 value -Pos4 value -Pos5 value";
                }
                else
                {
                    for (int i = 0; i < args.Length; i++)
                    {
                        if (args[i] == "-pos1")
                        {
                            if (!float.TryParse(args[i + 1], out Pos1))
                                return error;

                            try
                            {
                                EPos1 = Pos1;
                            }
                            catch
                            {
                                return "Unspecified error; failed to set lerp";
                            }

                        }

                        if (args[i] == "-pos2")
                        {
                            if (!float.TryParse(args[i + 1], out Pos2))
                                return error;

                            try
                            {
                                EPos2 = Pos2;
                            }
                            catch
                            {
                                return "Unspecified error; failed to set lerp";
                            }
                        }

                        if (args[i] == "-pos3")
                        {
                            if (!float.TryParse(args[i + 1], out Pos3))
                                return error;

                            try
                            {
                                EPos3 = Pos3;
                            }
                            catch
                            {
                                return "Unspecified error; failed to set lerp";
                            }
                        }

                        if (args[i] == "-pos4")
                        {
                            if (!float.TryParse(args[i + 1], out Pos4))
                                return error;

                            try
                            {
                                EPos4 = Pos4;
                            }
                            catch
                            {
                                return "Unspecified error; failed to set lerp";
                            }
                        }

                        if (args[i] == "-pos5")
                        {
                            if (!float.TryParse(args[i + 1], out Pos5))
                                return error;

                            try
                            {
                                EPos5 = Pos5;
                            }
                            catch
                            {
                                return "Unspecified error; failed to set lerp";
                            }
                        }
                    }
                }
                return string.Format("Frame 1 Pos: " + EPos1.ToString() + "Frame 2 Pos: " + EPos2.ToString() + "Frame 3 Pos: " + EPos3.ToString()+ "Frame 4 Pos: " + EPos4.ToString()+ "Frame 5 Pos: " + EPos5.ToString());
            }
        }

        public class ChangeFrameSpeed
        {
            public static readonly string name = "ChangeMovementMods";
            public static readonly string error = "Failed to set ChangeMovementMods - invalid setting or DaggerfallUnity singleton object";
            public static readonly string description = "Change sheathed and attack movement modifiers";
            public static readonly string usage = "ChangeMovementMods";

            public static float ESpeed1 { get; private set;}
            public static float ESpeed2 { get; private set;}
            public static float ESpeed3 { get; private set;}
            public static float ESpeed4 { get; private set;}
            public static float ESpeed5 { get; private set;}

            public static string Execute(params string[] args)
            {
                float Speed1;
                float Speed2;
                float Speed3;
                float Speed4;
                float Speed5;

                DaggerfallWorkshop.DaggerfallUnity daggerfallUnity = DaggerfallWorkshop.DaggerfallUnity.Instance;

                if (daggerfallUnity == null)
                    return error;

                if (args == null || args.Length < 1)
                {
                    return "-Pos1 value -Pos2 value -Pos3 value -Pos4 value -Pos5 value";
                }
                else
                {
                    for (int i = 0; i < args.Length; i++)
                    {
                        if (args[i] == "-speed1")
                        {
                            if (!float.TryParse(args[i + 1], out Speed1))
                                return error;

                            try
                            {
                                ESpeed1 = Speed1;
                            }
                            catch
                            {
                                return "Unspecified error; failed to set lerp";
                            }

                        }

                        if (args[i] == "-speed2")
                        {
                            if (!float.TryParse(args[i + 1], out Speed2))
                                return error;

                            try
                            {
                                ESpeed2 = Speed2;
                            }
                            catch
                            {
                                return "Unspecified error; failed to set lerp";
                            }
                        }

                        if (args[i] == "-speed3")
                        {
                            if (!float.TryParse(args[i + 1], out Speed3))
                                return error;

                            try
                            {
                                ESpeed3 = Speed3;
                            }
                            catch
                            {
                                return "Unspecified error; failed to set lerp";
                            }
                        }

                        if (args[i] == "-speed4")
                        {
                            if (!float.TryParse(args[i + 1], out Speed4))
                                return error;

                            try
                            {
                                ESpeed4 = Speed4;
                            }
                            catch
                            {
                                return "Unspecified error; failed to set lerp";
                            }
                        }

                        if (args[i] == "-speed5")
                        {
                            if (!float.TryParse(args[i + 1], out Speed5))
                                return error;

                            try
                            {
                                ESpeed5 = Speed5;
                            }
                            catch
                            {
                                return "Unspecified error; failed to set lerp";
                            }
                        }
                    }
                }
                return string.Format("Frame 1 Pos: " + ESpeed1.ToString() + "Frame 2 Pos: " + ESpeed2.ToString() + "Frame 3 Pos: " + ESpeed3.ToString() + "Frame 4 Pos: " + ESpeed4.ToString() + "Frame 5 Pos: " + ESpeed5.ToString());
            }
        }
    }
}