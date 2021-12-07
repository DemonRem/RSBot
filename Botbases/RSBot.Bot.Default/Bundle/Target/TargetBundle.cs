﻿using RSBot.Core;
using RSBot.Core.Objects;
using RSBot.Core.Objects.Spawn;
using System.Linq;

namespace RSBot.Bot.Default.Bundle.Target
{
    internal class TargetBundle : IBundle
    {
        /// <summary>
        /// Invokes this instance.
        /// </summary>
        public void Invoke()
        {
            if (Game.SelectedEntity?.Bionic?.State?.LifeState == LifeState.Alive &&
                !Game.SelectedEntity.Bionic.IsBehindObstacle)
                return;

            var monster = GetNearestEnemy();
            if (monster == null) 
                return;

            var character = monster.Character;
            if (character == null)
                return;

            var bionic = character.Bionic;
            if (bionic == null)
                return;

            var tracker = bionic.Tracker;
            if (tracker == null)
                return;

            //Check if the monster is still inside our range
            var distanceToCenter = tracker.Position.DistanceTo(Container.Bot.Area.CenterPosition);

            const int tolarance = 10;
            if (distanceToCenter > Container.Bot.Area.Radius + tolarance)
                return;

            //Move closer to the monster
            var distanceToPlayer = monster.Character.Bionic.Tracker.Position.DistanceTo(Game.Player.Tracker.Position);
            if (distanceToPlayer >= 80)
                Game.Player.Move(monster.Character.Bionic.Tracker.Position/*.BehindTo(monster.Character.Bionic.Tracker.Position, 20)*/);

            if (!Game.Player.SelectEntity(monster.Character.Bionic.UniqueId))
                Invoke();
        }

        /// <summary>
        /// Gets the nearest enemy.
        /// </summary>
        /// <returns></returns>
        private SpawnedMonster GetNearestEnemy()
        {
            return Game.Spawns.GetMonsters()
                    .Where(m => m != null &&
                           m.Character.Bionic.State.LifeState != LifeState.Dead &&
                           m.Character.Bionic.IsBehindObstacle == false &&
                           !Bundles.Avoidance.AvoidMonster(m.Rarity)
                          )
                    .OrderBy(m => m.Character.Bionic.Tracker?.Position.DistanceTo(Container.Bot.Area.CenterPosition))
                    .OrderByDescending(m => m.Character.Bionic.AttackingPlayer)
                    .OrderByDescending(m => Bundles.Avoidance.PreferMonster(m.Rarity))
                    .FirstOrDefault();
        }

        /// <summary>
        /// Refreshes this instance.
        /// </summary>
        public void Refresh()
        {
            //Nothing to do here
        }
    }
}