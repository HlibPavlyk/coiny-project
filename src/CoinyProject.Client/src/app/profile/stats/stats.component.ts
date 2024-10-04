import {Component, OnInit} from '@angular/core';
import {UserStatsModel} from "./stats.module";
import {UserService} from "../../services/user.service";
import {ActivatedRoute} from "@angular/router";

@Component({
  selector: 'app-stats',
  standalone: true,
  imports: [],
  templateUrl: './stats.component.html',
  styleUrl: './stats.component.css'
})
export class StatsComponent implements OnInit {
  userStats: UserStatsModel | null = null;
  isCurrentUser: boolean = false;

  constructor(
    private userService: UserService,
    private route: ActivatedRoute // Inject ActivatedRoute to access route parameters
  ) {}

  ngOnInit(): void {
    this.route.queryParams.subscribe(queryParams => {
      this.isCurrentUser = queryParams['my'] === 'true'; // перевіряємо, чи це альбоми поточного користувача
    });

    this.fetchUserStats();
  }

  fetchUserStats(): void {
    // Check if there is an ID in the route parameters
    const userId = this.route.snapshot.paramMap.get('id');

    // Fetch user statistics based on whether userId exists or not
    this.userService.getUserStats(userId).subscribe(
      (stats) => {
        this.userStats = stats;
      },
      (error) => {
        console.error('Error fetching user stats:', error);
      }
    );
  }
}
