import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Router } from '@angular/router';
import { AuthService } from '../../../core/auth/auth.service';

@Component({
  selector: 'app-login',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './login.component.html'
})
export class LoginComponent {
  email = '';
  senha = '';
  mensagemErro: string | null = null;
  carregando = false;

  constructor(private authService: AuthService, private router: Router) {}

  entrar(): void {
    this.mensagemErro = null;
    this.carregando = true;

    this.authService.login(this.email, this.senha).subscribe({
      next: () => {
        this.carregando = false;
        this.router.navigateByUrl('/');
      },
      error: (erro) => {
        this.carregando = false;
        this.mensagemErro = erro.error?.message ?? 'Erro ao tentar fazer login.';
      }
    });
  }
}
