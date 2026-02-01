import {Component, OnInit} from '@angular/core';
import {UserStatsModel} from "./stats.module";
import {UserService} from "../../../core/services/user.service";
import {ActivatedRoute} from "@angular/router";
import {AuthService} from "../../../core/services/auth.service";

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
    private authService: AuthService,
    private route: ActivatedRoute
  ) {}

  ngOnInit(): void {
    this.route.queryParams.subscribe(queryParams => {
      this.isCurrentUser = queryParams['my'] === 'true';
    });

    this.fetchUserStats();
  }

  fetchUserStats(): void {
    // Check if there is an ID in the route parameters
    let userId = this.route.snapshot.paramMap.get('id');

    // If no userId in route, get current user's ID from auth service
    if (!userId) {
      const currentUser = this.authService.getUser();
      userId = currentUser?.id || null;
    }

    // Fetch user statistics if we have a valid userId
    if (userId) {
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
}
