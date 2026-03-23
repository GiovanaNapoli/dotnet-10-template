using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Infrastructure.Templates
{
    public class EmailTemplates
    {
        public static string MfaCode(string code) => $"""
        <div style="font-family: Arial, sans-serif; max-width: 600px; margin: 0 auto;">
            <h2>Código de verificação</h2>
            <p>Use o código abaixo para concluir seu login:</p>
            <div style="font-size: 32px; font-weight: bold; letter-spacing: 8px; 
                        padding: 20px; background: #f4f4f4; text-align: center;">
                {code}
            </div>
            <p style="color: #999; font-size: 12px;">
                Este código expira em 10 minutos. Não compartilhe com ninguém.
            </p>
        </div>
        """;

        public static string PasswordReset(string resetLink) => $"""
        <div style="font-family: Arial, sans-serif; max-width: 600px; margin: 0 auto;">
            <h2>Redefinição de senha</h2>
            <p>Clique no botão abaixo para redefinir sua senha:</p>
            <a href="{resetLink}" 
               style="display: inline-block; padding: 12px 24px; background: #007bff; 
                      color: white; text-decoration: none; border-radius: 4px;">
                Redefinir senha
            </a>
            <p style="color: #999; font-size: 12px;">
                Este link expira em 1 hora. Se não solicitou, ignore este email.
            </p>
        </div>
        """;
    }
}