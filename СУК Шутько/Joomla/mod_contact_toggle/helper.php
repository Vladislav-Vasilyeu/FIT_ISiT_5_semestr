<?php
class ModContactToggleHelper {
    public static function sendMessage($params) {
        $name = JFactory::getApplication()->input->post->get('contact_name', '', 'string');
        $subject = JFactory::getApplication()->input->post->get('contact_subject', '', 'string');
        $message = JFactory::getApplication()->input->post->get('contact_message', '', 'raw');
        $captcha = JFactory::getApplication()->input->post->get('contact_captcha', '', 'string');

        if ($captcha !== '4') {
            return ['success' => false, 'message' => 'Ошибка капчи! 2+2=4'];
        }

        $mailer = JFactory::getMailer();
        $config = JFactory::getConfig();
        $sender = [$config->get('mailfrom'), $config->get('fromname')];
        $mailer->setSender($sender);
        $mailer->addRecipient($config->get('mailfrom'));
        $mailer->setSubject('Обратная связь: ' . htmlspecialchars($subject));
        $body = "Имя: $name\nТема: $subject\n\nСообщение:\n$message";
        $mailer->setBody($body);

        if ($mailer->Send()) {
            return ['success' => true, 'message' => $params->get('success_message')];
        }
        return ['success' => false, 'message' => 'Ошибка отправки'];
    }
}